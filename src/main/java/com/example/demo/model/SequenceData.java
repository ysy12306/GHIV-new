package com.example.demo.model;

import lombok.Data;

@Data
public class SequenceData {
    private Integer id;
    private String name;
    private String sequence;
    private String discreteTraits;
    private String continuousTraits; 
    private Integer quantity;
    private String organism;
} 