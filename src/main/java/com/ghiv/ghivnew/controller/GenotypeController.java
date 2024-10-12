package com.ghiv.ghivnew.controller;

import com.ghiv.ghivnew.service.TimerService;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.io.File;

@RestController
//鉴定分型①
public class GenotypeController {
    private final TimerService timer = new TimerService();
    @RequestMapping ("/genotype")
    public void execute(){
        timer.Timer7();
        recreateDirectories();
        //quick_barcode
    }
    public void recreateDirectories() {
        String currentDirectory = "";//修改为工作目录
        deleteDir(new File(currentDirectory, "temp/temp_quick"));
        new File(currentDirectory, "temp/temp_quick").mkdirs();

        deleteDir(new File(currentDirectory, "temp/temp_data"));
        new File(currentDirectory, "temp/temp_data").mkdirs();
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
