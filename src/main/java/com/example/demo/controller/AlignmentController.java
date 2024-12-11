package com.example.demo.controller;

import com.example.demo.model.AlignmentType;
import com.example.demo.model.SequenceData;
import com.example.demo.service.AlignmentService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/alignment")
public class AlignmentController {

    @Autowired
    private AlignmentService alignmentService;

    @PostMapping("/align")
    public ResponseEntity<String> alignSequences(
            @RequestBody List<SequenceData> sequences,
            @RequestParam AlignmentType alignmentType) {
        try {
            String alignedSequences = alignmentService.alignSequences(sequences, alignmentType);
            return ResponseEntity.ok(alignedSequences);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }
} 