package com.example.demo.model;

public enum NetworkType {
    MODIFIED_TCS("modified_tcs"),
    MJN("mjn"),
    MSN("msn");

    private final String parameter;

    NetworkType(String parameter) {
        this.parameter = parameter;
    }

    public String getParameter() {
        return parameter;
    }
} 