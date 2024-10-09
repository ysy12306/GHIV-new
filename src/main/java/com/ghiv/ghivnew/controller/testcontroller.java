package com.ghiv.ghivnew.controller;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class testcontroller {
    @GetMapping("/test")
    public String test() {
        return "Hello World";
    }
}
