package com.example.demo.service;

import com.example.demo.model.AlignmentType;
import com.example.demo.model.SequenceData;
import org.springframework.stereotype.Service;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.List;
import java.util.UUID;

@Service
public class AlignmentService {

    private static final String TEMP_DIR = System.getProperty("java.io.tmpdir");
    private static final String ANALYSIS_DIR = "analysis";
    
    public String alignSequences(List<SequenceData> sequences, AlignmentType alignmentType) throws Exception {
        // 生成唯一的任务ID
        String taskId = UUID.randomUUID().toString();
        
        // 创建临时输入文件
        String inputPath = createInputFile(sequences, taskId);
        
        // 创建临时输出文件路径
        String outputPath = TEMP_DIR + "/" + taskId + "_aligned.fasta";
        
        // 执行比对
        if (alignmentType.toString().startsWith("MAFFT")) {
            runMafftAlignment(inputPath, outputPath, alignmentType);
        } else {
            runMuscleAlignment(inputPath, outputPath, alignmentType);
        }
        
        // 读取比对结果
        String alignedSequences = new String(Files.readAllBytes(Paths.get(outputPath)), StandardCharsets.UTF_8);
        
        // 清理临时文件
        Files.deleteIfExists(Paths.get(inputPath));
        Files.deleteIfExists(Paths.get(outputPath));
        
        return alignedSequences;
    }
    
    private String createInputFile(List<SequenceData> sequences, String taskId) throws IOException {
        String inputPath = TEMP_DIR + "/" + taskId + ".fasta";
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(inputPath))) {
            for (SequenceData seq : sequences) {
                writer.write(">T" + seq.getId() + "\n");
                writer.write(seq.getSequence() + "\n");
            }
        }
        return inputPath;
    }
    
    private void runMafftAlignment(String inputPath, String outputPath, AlignmentType type) throws Exception {
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/mafft-win/mafft.bat",
            type.getParameter(),
            "--inputorder",
            inputPath
        );
        
        // 设置工作目录
        pb.directory(new File(ANALYSIS_DIR + "/mafft-win/"));
        
        // 重定向输出
        pb.redirectOutput(new File(outputPath));
        
        Process process = pb.start();
        int exitCode = process.waitFor();
        
        if (exitCode != 0) {
            throw new RuntimeException("MAFFT alignment failed with exit code: " + exitCode);
        }
    }
    
    private void runMuscleAlignment(String inputPath, String outputPath, AlignmentType type) throws Exception {
        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/muscle5.1.win64.exe",
            type.getParameter(),
            inputPath,
            "-output",
            outputPath
        );
        
        // 设置工作目录
        pb.directory(new File(ANALYSIS_DIR));
        
        Process process = pb.start();
        int exitCode = process.waitFor();
        
        if (exitCode != 0) {
            throw new RuntimeException("MUSCLE alignment failed with exit code: " + exitCode);
        }
    }
} 