package com.example.demo.service;

import com.example.demo.model.SequenceData;
import org.springframework.stereotype.Service;

import java.io.*;
import java.util.List;
import java.util.UUID;

@Service
public class SequenceProcessService {
    
    private static final String ANALYSIS_DIR = "analysis";
    private static final String TEMP_DIR = System.getProperty("java.io.tmpdir");

    public List<SequenceData> cleanSequences(List<SequenceData> sequences) throws Exception {
        String taskId = UUID.randomUUID().toString();
        String inputPath = createInputFile(sequences, taskId);
        String outputPath = TEMP_DIR + "/" + taskId + "_clean.fasta";

        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/clean_fasta.exe",
            "-i", inputPath,
            "-o", outputPath
        );
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();

        return readCleanedSequences(outputPath);
    }

    public List<SequenceData> getSequenceInfo(List<SequenceData> sequences) throws Exception {
        String taskId = UUID.randomUUID().toString();
        String inputPath = createInputFile(sequences, taskId);
        String outputPath = TEMP_DIR + "/" + taskId + "_info.csv";

        ProcessBuilder pb = new ProcessBuilder(
            ANALYSIS_DIR + "/seq_info.exe",
            "-i", inputPath,
            "-o", outputPath
        );
        pb.directory(new File(ANALYSIS_DIR));
        Process process = pb.start();
        process.waitFor();

        return readSequenceInfo(outputPath);
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

    private List<SequenceData> readCleanedSequences(String filePath) {
        // 实现读取清理后的序列
        return null;
    }

    private List<SequenceData> readSequenceInfo(String filePath) {
        // 实现读取序列信息
        return null;
    }
} 