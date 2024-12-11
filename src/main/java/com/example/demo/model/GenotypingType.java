package com.example.demo.model;

public enum GenotypingType {
    ONLINE("online"),
    LOCAL("local");
    
    private final String type;
    
    GenotypingType(String type) {
        this.type = type;
    }
    
    public String getType() {
        return type;
    }
} 