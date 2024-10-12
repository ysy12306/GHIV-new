package com.ghiv.ghivnew.controller;

import org.springframework.core.io.Resource;
import org.springframework.core.io.UrlResource;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.net.MalformedURLException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.UUID;

@RestController
@RequestMapping("/api/files")
public class FileUploadController {

    @PostMapping("/upload")
    public ResponseEntity<String> handleFileUpload(@RequestParam("file") MultipartFile file) {
        if (file.isEmpty()) {
            return ResponseEntity.status(HttpStatus.BAD_REQUEST).body("File is empty");
        }

        try {
            // 生成唯一标识符
            String fileId = UUID.randomUUID().toString();
            String fileName = fileId + "_" + file.getOriginalFilename();
            // 保存原始文件
            Path originalFilePath = Paths.get("uploads/" + fileName);
            Files.copy(file.getInputStream(), originalFilePath, StandardCopyOption.REPLACE_EXISTING);

            /*
            // 模拟文件处理后的结果保存
            String processedFileName = fileId + "_processed_" + file.getOriginalFilename();
            Path processedFilePath = Paths.get("uploads/processed/" + processedFileName);
            // 在这里进行文件处理（例如格式转换、压缩等），保存处理后的文件
            Files.copy(file.getInputStream(), processedFilePath, StandardCopyOption.REPLACE_EXISTING);
            */

            // 返回唯一标识符，供后续下载使用
            return ResponseEntity.ok(fileId);
        } catch (IOException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("File upload failed");
        }
    }

    @GetMapping("/download/{fileId}")
    public ResponseEntity<Resource> downloadFile(@PathVariable String fileId) {
        try {
            // 找到处理后的文件
            Path filePath = Paths.get("/" + fileId + "/");
            Resource fileResource = new UrlResource(filePath.toUri());

            if (!fileResource.exists()) {
                return ResponseEntity.status(HttpStatus.NOT_FOUND).body(null);
            }

            return ResponseEntity.ok()
                    .header(HttpHeaders.CONTENT_DISPOSITION, "attachment; filename=\"" + fileResource.getFilename() + "\"")
                    .body(fileResource);
        } catch (MalformedURLException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(null);
        }
    }
}


