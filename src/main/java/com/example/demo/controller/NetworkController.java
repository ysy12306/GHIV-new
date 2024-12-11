package com.example.demo.controller;

import com.example.demo.model.NetworkType;
import com.example.demo.model.SequenceData;
import com.example.demo.service.NetworkService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/network")
public class NetworkController {

    @Autowired
    private NetworkService networkService;

    @PostMapping("/construct")
    public ResponseEntity<String> constructNetwork(
            @RequestBody List<SequenceData> sequences,
            @RequestParam NetworkType networkType,
            @RequestParam(required = false) Double epsilon) {
        try {
            String result = networkService.constructNetwork(sequences, networkType, epsilon);
            return ResponseEntity.ok(result);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }
} 