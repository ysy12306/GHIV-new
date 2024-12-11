package com.example.demo.controller;

import com.example.demo.model.GenotypingResult;
import com.example.demo.model.GenotypingType;
import com.example.demo.model.SequenceData;
import com.example.demo.service.GenotypingService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/genotyping")
public class GenotypingController {

    @Autowired
    private GenotypingService genotypingService;

    @PostMapping("/analyze")
    public ResponseEntity<List<GenotypingResult>> analyzeGenotype(
            @RequestBody List<SequenceData> sequences,
            @RequestParam GenotypingType type) {
        try {
            List<GenotypingResult> results = genotypingService.genotype(sequences, type);
            return ResponseEntity.ok(results);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }
} 