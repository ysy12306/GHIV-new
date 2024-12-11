package com.example.demo.service;

import com.example.demo.model.GenotypingType;
import com.example.demo.model.GenotypingResult;
import com.example.demo.model.SequenceData;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.stereotype.Service;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service
public class GenotypingService {

    private static final String TEMP_DIR = System.getProperty("java.io.tmpdir");
    private static final String ANALYSIS_DIR = "analysis";
    private static final String HISTORY_DIR = "history";
    private final ObjectMapper objectMapper = new ObjectMapper();

    public List<GenotypingResult> genotype(List<SequenceData> sequences, GenotypingType type) throws Exception {
        String taskId = UUID.randomUUID().toString();
        
        // 1. 创建输入文件
        String inputPath = createInputFile(sequences, taskId);
        
        // 2. 执行分型分析
        String outputPath;
        if (type == GenotypingType.ONLINE) {
            outputPath = runOnlineGenotyping(inputPath, taskId);
        } else {
            outputPath = runLocalGenotyping(inputPath, taskId);
        }
        
        // 3. 解析结果
        List<GenotypingResult> results = parseResults(outputPath);
        
        // 4. 保存到历史记录
        saveToHistory(taskId, results, type);
        
        return results;
    }

    private String createInputFile(List<SequenceData> sequences, String taskId) throws IOException {
        String inputPath = TEMP_DIR + "/" + taskId + ".fasta";
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(inputPath))) {
            for (SequenceData seq : sequences) {
                writer.write(">" + seq.getName() + "\n");
                writer.write(seq.getSequence() + "\n");
            }
        }
        return inputPath;
    }

    private String runOnlineGenotyping(String inputPath, String taskId) throws Exception {
        String outputPath = TEMP_DIR + "/" + taskId + ".json";
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/sierra.exe",
            "fasta",
            inputPath,
            "-o",
            outputPath
        );
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();
        return outputPath + ".0.json";
    }

    private String runLocalGenotyping(String inputPath, String taskId) throws Exception {
        String outputPath = TEMP_DIR + "/" + taskId + "_results.json";
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/sierralocal.exe",
            inputPath,
            "-alignment",
            "nuc"
        );
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();
        return outputPath;
    }

    private List<GenotypingResult> parseResults(String jsonPath) throws Exception {
        List<GenotypingResult> results = new ArrayList<>();
        String jsonContent = new String(Files.readAllBytes(Paths.get(jsonPath)), StandardCharsets.UTF_8);
        JsonNode rootArray = objectMapper.readTree(jsonContent);
        
        for (JsonNode resultNode : rootArray) {
            GenotypingResult result = new GenotypingResult();
            
            // 解析基本信息
            result.setHeader(resultNode.path("inputSequence").path("header").asText());
            result.setSubtypeText(resultNode.path("subtypeText").asText());
            
            // 解析验证结果
            JsonNode validationResults = resultNode.path("validationResults");
            if (validationResults.isArray() && validationResults.size() > 0) {
                JsonNode firstValidation = validationResults.get(0);
                result.setValidationLevel(firstValidation.path("level").asText());
                result.setValidationMessage(firstValidation.path("message").asText()
                    .replace(" positions were not sequenced or aligned:", "个位点没有测序或排序, 包括: ")
                    .replace(" PR ", " 蛋白酶(PR) ")
                    .replace(" RT ", " 逆转录酶(RT) ")
                    .replace(". Of them, ", ". 其中有")
                    .replace(" are at drug-resistance positions", "个位于耐药性区域"));
            }
            
            // 解析药物抗性信息
            StringBuilder drugInfo = new StringBuilder();
            JsonNode drugResistance = resultNode.path("drugResistance");
            for (JsonNode dr : drugResistance) {
                JsonNode drugScores = dr.path("drugScores");
                for (JsonNode ds : drugScores) {
                    String drugClass = ds.path("drugClass").path("name").asText();
                    String drugName = ds.path("drug").path("name").asText();
                    String drugAbbr = ds.path("drug").path("displayAbbr").asText();
                    String score = ds.path("score").asText();
                    String text = ds.path("text").asText()
                        .replace("Susceptible", "敏感")
                        .replace("Potential ", "潜在")
                        .replace("Low-Level Resistance", "低抵抗")
                        .replace("Intermediate Resistance", "中抵抗")
                        .replace("High-Level Resistance", "高抵抗");
                    
                    drugInfo.append(String.format("%s,%s,%s,%s,%s\n", 
                        drugClass, drugName, drugAbbr, score, text));
                }
            }
            result.setDrugInfo(drugInfo.toString());
            
            results.add(result);
        }
        
        return results;
    }

    private void saveToHistory(String taskId, List<GenotypingResult> results, GenotypingType type) throws IOException {
        // 保存HTML报告
        String htmlPath = HISTORY_DIR + "/" + taskId + ".html";
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(htmlPath))) {
            writer.write(generateHtmlReport(taskId, results));
        }
        
        // 更新历史记录
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(HISTORY_DIR + "/history.csv", true))) {
            String timestamp = LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm"));
            writer.write(String.format("%s,%s,Genotype (%s)\n", timestamp, taskId, type.getType()));
        }
    }

    private String generateHtmlReport(String taskId, List<GenotypingResult> results) {
        StringBuilder html = new StringBuilder();
        html.append("<!DOCTYPE html>\n<html>\n<head>\n<title>Genotyping Report</title>\n</head>\n<body>\n");
        
        for (GenotypingResult result : results) {
            html.append("<h2>").append(result.getHeader()).append("</h2>\n");
            html.append("<h3>分型结果: ").append(result.getSubtypeText()).append("</h3>\n");
            html.append("<h3>验证信息</h3>\n");
            html.append("<p>级别: ").append(result.getValidationLevel()).append("</p>\n");
            html.append("<p>信息: ").append(result.getValidationMessage()).append("</p>\n");
            
            if (result.getDrugInfo() != null && !result.getDrugInfo().isEmpty()) {
                html.append("<h3>药物抗性信息</h3>\n");
                html.append("<table border='1'>\n");
                html.append("<tr><th>药物类别</th><th>药物名称</th><th>缩写</th><th>得分</th><th>结果</th></tr>\n");
                
                String[] drugLines = result.getDrugInfo().split("\n");
                for (String line : drugLines) {
                    if (!line.trim().isEmpty()) {
                        String[] parts = line.split(",");
                        html.append("<tr>");
                        for (String part : parts) {
                            html.append("<td>").append(part).append("</td>");
                        }
                        html.append("</tr>\n");
                    }
                }
                html.append("</table>\n");
            }
        }
        
        html.append("</body>\n</html>");
        return html.toString();
    }
} 