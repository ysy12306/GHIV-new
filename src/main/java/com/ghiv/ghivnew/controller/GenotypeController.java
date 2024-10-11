package com.ghiv.ghivnew.controller;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
//鉴定分型①
public class GenotypeController {
    @PostMapping("/genotype")
    public String forwardToSecond() {
        return "forward:/upload";
    }
}
