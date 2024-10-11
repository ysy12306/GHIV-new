package com.ghiv.ghivnew.controller;

import jakarta.servlet.http.HttpServletRequest;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.multipart.MultipartFile;

import java.io.File;
import java.io.IOException;

@RestController
public class FileUploadController {
    @PostMapping("/upload")
    public String up(String filename, MultipartFile file, HttpServletRequest request) throws IOException {
        String path = request.getServletContext().getRealPath("/upload");
        saveFile(file, path);
        return "File uploaded successfully";
    }

    private void saveFile(MultipartFile file, String path) throws IOException {
        File dir = new File(path);
        if (!dir.exists()) {
            dir.mkdirs();
        }

        File pfile = new File(path + file.getOriginalFilename());
        file.transferTo(pfile);
    }
}
