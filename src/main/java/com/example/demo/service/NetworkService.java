package com.example.demo.service;

import com.example.demo.model.NetworkType;
import com.example.demo.model.SequenceData;
import org.springframework.stereotype.Service;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.List;
import java.util.UUID;

@Service
public class NetworkService {

    private static final String TEMP_DIR = System.getProperty("java.io.tmpdir");
    private static final String ANALYSIS_DIR = "analysis";
    private static final String HISTORY_DIR = "history";

    public String constructNetwork(List<SequenceData> sequences, NetworkType networkType, Double epsilon) throws Exception {
        String taskId = UUID.randomUUID().toString();
        
        // 1. 创建输入文件
        String inputPath = createInputFile(sequences, taskId);
        
        // 2. 执行序列比对(如果序列未对齐)
        String alignedPath = alignSequences(inputPath, taskId);
        
        // 3. 生成单倍型数据
        String hapPath = generateHaplotypes(alignedPath, taskId);
        
        // 4. 构建网络
        String networkPath = buildNetwork(hapPath, taskId, networkType, epsilon);
        
        // 5. 生成可视化数据
        String visualPath = generateVisualization(networkPath, taskId);
        
        // 6. 读取结果
        String result = new String(Files.readAllBytes(Paths.get(visualPath)), StandardCharsets.UTF_8);
        
        // 7. 保存到历史记录
        saveToHistory(taskId, result);
        
        return result;
    }

    private String createInputFile(List<SequenceData> sequences, String taskId) throws IOException {
        String inputPath = TEMP_DIR + "/" + taskId + ".fasta";
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(inputPath))) {
            for (SequenceData seq : sequences) {
                writer.write(">" + seq.getName() + "=" + seq.getContinuousTraits() + "=" + 
                           seq.getDiscreteTraits() + "$SPLIT$" + seq.getOrganism() + "\n");
                writer.write(seq.getSequence() + "\n");
            }
        }
        return inputPath;
    }

    private String alignSequences(String inputPath, String taskId) throws Exception {
        String outputPath = TEMP_DIR + "/" + taskId + "_aln.fasta";
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/mafft-win/mafft.bat",
            "--retree", "2",
            "--inputorder",
            inputPath
        );
        pb.directory(new File(ANALYSIS_DIR + "/mafft-win/"));
        pb.redirectOutput(new File(outputPath));
        Process process = pb.start();
        process.waitFor();
        return outputPath;
    }

    private String generateHaplotypes(String alignedPath, String taskId) throws Exception {
        String outputPath = TEMP_DIR + "/" + taskId + "_hap";
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/fastHaN_win_intel.exe",
            "-i", alignedPath,
            "-o", outputPath
        );
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();
        return outputPath + ".phy";
    }

    private String buildNetwork(String hapPath, String taskId, NetworkType networkType, Double epsilon) throws Exception {
        String outputPath = TEMP_DIR + "/" + taskId + ".gml";
        List<String> command = new ArrayList<>();
        command.add(ANALYSIS_DIR + "/fastHaN_win_intel.exe");
        command.add(networkType.getParameter());
        command.add("-i");
        command.add(hapPath);
        command.add("-o");
        command.add(outputPath);
        
        if (epsilon != null && (networkType == NetworkType.MSN || networkType == NetworkType.MJN)) {
            command.add("-e");
            command.add(epsilon.toString());
        }
        
        ProcessBuilder pb = new ProcessBuilder(command);
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();
        return outputPath;
    }

    private String generateVisualization(String networkPath, String taskId) throws Exception {
        String outputPath = HISTORY_DIR + "/" + taskId + ".js";
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/GenNetworkConfig.exe",
            networkPath,
            taskId + ".json",
            taskId + ".meta",
            taskId
        );
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();
        return outputPath;
    }

    private void saveToHistory(String taskId, String result) throws IOException {
        // 保存网络分析结果到历史记录
        String historyPath = HISTORY_DIR + "/" + taskId + ".html";
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(historyPath))) {
            writer.write(generateHtmlTemplate(taskId, result));
        }
        
        // 更新历史记录列表
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(HISTORY_DIR + "/history.csv", true))) {
            writer.write(String.format("%s,%s,%s\n", 
                java.time.LocalDateTime.now().format(java.time.format.DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm")),
                taskId,
                "Network Analysis"
            ));
        }
    }

    private String generateHtmlTemplate(String taskId, String result) {
        return "<!DOCTYPE html>\n" +
               "<html>\n" +
               "<head>\n" +
               "    <title>Network Analysis Result</title>\n" +
               "    <script src=\"" + taskId + ".js\"></script>\n" +
               "</head>\n" +
               "<body>\n" +
               "    <div id=\"network-container\"></div>\n" +
               "    <script>\n" +
               "        document.addEventListener('DOMContentLoaded', function() {\n" +
               "            renderNetwork(" + result + ");\n" +
               "        });\n" +
               "    </script>\n" +
               "</body>\n" +
               "</html>";
    }
} 