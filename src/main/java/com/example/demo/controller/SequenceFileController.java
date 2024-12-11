package com.example.demo.controller;

import com.example.demo.service.SequenceFileService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.util.List;

@RestController
@RequestMapping("/api/sequence/file")
public class SequenceFileController {

    @Autowired
    private SequenceFileService sequenceFileService;

    @PostMapping("/split")
    public ResponseEntity<Void> splitFile(@RequestParam String inputPath, 
                                        @RequestParam int chunkSize,
                                        @RequestParam String outputDir) {
        try {
            sequenceFileService.splitSequenceFile(inputPath, chunkSize, outputDir);
            return ResponseEntity.ok().build();
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }

    @PostMapping("/merge")
    public ResponseEntity<Void> mergeFiles(@RequestBody List<String> inputFiles,
                                         @RequestParam String outputPath) {
        try {
            sequenceFileService.mergeSequenceFiles(inputFiles, outputPath);
            return ResponseEntity.ok().build();
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }

    @PostMapping("/convert")
    public ResponseEntity<Void> convertCsvToFasta(@RequestParam String csvPath,
                                                @RequestParam String fastaPath) {
        try {
            sequenceFileService.convertCsvToFasta(csvPath, fastaPath);
            return ResponseEntity.ok().build();
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }
} 