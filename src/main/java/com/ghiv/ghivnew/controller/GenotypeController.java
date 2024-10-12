package com.ghiv.ghivnew.controller;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.File;

import static org.apache.catalina.startup.ExpandWar.deleteDir;

@RestController
@RequestMapping ("/genotype")
//鉴定分型①
public class GenotypeController {
    public String forwardToTimer7() {
        return "forward:/timer7";
    }
    public void recreateDirectories() {
        String currentDirectory = "";//修改为工作目录
        deleteDir(new File(currentDirectory, "temp/temp_quick"));
        new File(currentDirectory, "temp/temp_quick").mkdirs();

        deleteDir(new File(currentDirectory, "temp/temp_data"));
        new File(currentDirectory, "temp/temp_data").mkdirs();
    }
    public String forwardToFileUpload() {
        return "forward:/upload";
    }

    private void deleteDir(File dir) {
        if (dir.isDirectory()) {
            File[] children = dir.listFiles();
            if (children != null) {
                for (File child : children) {
                    deleteDir(child);
                }
            }
        }
        dir.delete();
    }
}
