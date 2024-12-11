package com.example.demo.controller;

import com.example.demo.model.SequenceData;
import com.example.demo.service.SequenceProcessService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/sequence/process")
public class SequenceProcessController {

    @Autowired
    private SequenceProcessService sequenceProcessService;

    @PostMapping("/clean")
    public ResponseEntity<List<SequenceData>> cleanSequences(@RequestBody List<SequenceData> sequences) {
        try {
            List<SequenceData> cleanedSequences = sequenceProcessService.cleanSequences(sequences);
            return ResponseEntity.ok(cleanedSequences);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }

    @PostMapping("/info")
    public ResponseEntity<List<SequenceData>> getSequenceInfo(@RequestBody List<SequenceData> sequences) {
        try {
            List<SequenceData> sequenceInfo = sequenceProcessService.getSequenceInfo(sequences);
            return ResponseEntity.ok(sequenceInfo);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }
} 