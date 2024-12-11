package com.example.demo.model;

import lombok.Data;

@Data
public class GenotypingResult {
    private String header;
    private String subtypeText;
    private String validationLevel;
    private String validationMessage;
    private String drugInfo;
} 