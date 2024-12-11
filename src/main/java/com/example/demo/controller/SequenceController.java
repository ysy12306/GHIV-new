package com.example.demo.controller;

import com.example.demo.model.SequenceData;
import com.example.demo.service.SequenceService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.io.File;
import java.util.List;

@RestController
@RequestMapping("/api/sequence")
public class SequenceController {

    @Autowired
    private SequenceService sequenceService;

    // 上传并加载序列文件
    @PostMapping("/upload")
    public ResponseEntity<List<SequenceData>> uploadSequence(@RequestParam("file") MultipartFile file) {
        try {
            // 保存上传的文件
            String tempFilePath = System.getProperty("java.io.tmpdir") + "/" + file.getOriginalFilename();
            file.transferTo(new File(tempFilePath));
            
            // 加载序列数据
            List<SequenceData> sequences = sequenceService.loadSequenceData(tempFilePath);
            
            return ResponseEntity.ok(sequences);
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }

    // 保存序列数据
    @PostMapping("/save") 
    public ResponseEntity<Void> saveSequence(@RequestBody List<SequenceData> sequences, 
                                           @RequestParam String filePath) {
        try {
            sequenceService.saveSequenceData(filePath, sequences);
            return ResponseEntity.ok().build();
        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }
} 