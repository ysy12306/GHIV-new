package com.example.demo.service;

import com.example.demo.model.SequenceData;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.*;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

@Service
public class SequenceFileService {

    private static final String TEMP_DIR = System.getProperty("java.io.tmpdir");

    public void splitSequenceFile(String inputPath, int chunkSize, String outputDir) throws IOException {
        List<SequenceData> sequences = readFastaFile(inputPath);
        int fileCount = (sequences.size() + chunkSize - 1) / chunkSize;
        
        for (int i = 0; i < fileCount; i++) {
            int start = i * chunkSize;
            int end = Math.min(start + chunkSize, sequences.size());
            List<SequenceData> chunk = sequences.subList(start, end);
            
            String outputPath = outputDir + "/chunk_" + (i + 1) + ".fasta";
            writeFastaFile(chunk, outputPath);
        }
    }

    public void mergeSequenceFiles(List<String> inputFiles, String outputPath) throws IOException {
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(outputPath))) {
            for (String inputFile : inputFiles) {
                List<SequenceData> sequences = readFastaFile(inputFile);
                for (SequenceData seq : sequences) {
                    writer.write(">" + seq.getName() + "\n");
                    writer.write(seq.getSequence() + "\n");
                }
            }
        }
    }

    public void convertCsvToFasta(String csvPath, String fastaPath) throws IOException {
        List<SequenceData> sequences = readCsvFile(csvPath);
        writeFastaFile(sequences, fastaPath);
    }

    private List<SequenceData> readFastaFile(String filePath) throws IOException {
        List<SequenceData> sequences = new ArrayList<>();
        try (BufferedReader reader = new BufferedReader(new FileReader(filePath))) {
            String line;
            SequenceData currentSeq = null;
            StringBuilder sequence = new StringBuilder();
            
            while ((line = reader.readLine()) != null) {
                if (line.startsWith(">")) {
                    if (currentSeq != null) {
                        currentSeq.setSequence(sequence.toString());
                        sequences.add(currentSeq);
                    }
                    currentSeq = new SequenceData();
                    currentSeq.setName(line.substring(1));
                    sequence = new StringBuilder();
                } else {
                    sequence.append(line.trim());
                }
            }
            if (currentSeq != null) {
                currentSeq.setSequence(sequence.toString());
                sequences.add(currentSeq);
            }
        }
        return sequences;
    }

    private void writeFastaFile(List<SequenceData> sequences, String filePath) throws IOException {
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(filePath))) {
            for (SequenceData seq : sequences) {
                writer.write(">" + seq.getName() + "\n");
                writer.write(seq.getSequence() + "\n");
            }
        }
    }

    private List<SequenceData> readCsvFile(String filePath) throws IOException {
        List<SequenceData> sequences = new ArrayList<>();
        try (BufferedReader reader = new BufferedReader(new FileReader(filePath))) {
            String line = reader.readLine(); // Skip header
            while ((line = reader.readLine()) != null) {
                String[] parts = line.split(",");
                SequenceData seq = new SequenceData();
                seq.setName(parts[0]);
                seq.setSequence(parts[1]);
                sequences.add(seq);
            }
        }
        return sequences;
    }
} 