package com.example.demo.model;

public enum AlignmentType {
    MAFFT_AUTO("--auto"),
    MAFFT_FFTNS1("--retree 1"),
    MAFFT_FFTNS2("--retree 2"), 
    MAFFT_GINSI("--globalpair --maxiterate 16"),
    MAFFT_EINSI("--genafpair --maxiterate 16"),
    MUSCLE_PPP("-align"),
    MUSCLE_SUPER5("-super5");

    private final String parameter;

    AlignmentType(String parameter) {
        this.parameter = parameter;
    }

    public String getParameter() {
        return parameter;
    }
} 