package com.example.demo.service;

import com.example.demo.model.SequenceData;
import org.springframework.stereotype.Service;
import java.io.*;
import java.util.*;

@Service
public class SequenceService {

    // 加载序列数据
    public List<SequenceData> loadSequenceData(String filePath) throws IOException {
        List<SequenceData> sequences = new ArrayList<>();
        
        try (BufferedReader reader = new BufferedReader(new FileReader(filePath))) {
            String line;
            // 跳过表头
            reader.readLine();
            
            while ((line = reader.readLine()) != null) {
                if (!line.trim().isEmpty()) {
                    String[] data = line.split(",");
                    SequenceData seq = new SequenceData();
                    seq.setId(Integer.parseInt(data[0]));
                    seq.setName(data[1]);
                    seq.setSequence(data[2]);
                    seq.setDiscreteTraits(data[3]);
                    seq.setContinuousTraits(data[4]);
                    seq.setQuantity(Integer.parseInt(data[5]));
                    seq.setOrganism(data.length > 6 ? data[6] : "");
                    sequences.add(seq);
                }
            }
        }
        
        return sequences;
    }

    // 保存序列数据
    public void saveSequenceData(String filePath, List<SequenceData> sequences) throws IOException {
        try (BufferedWriter writer = new BufferedWriter(new FileWriter(filePath))) {
            // 写入表头
            writer.write("ID,Name,Sequence,DiscreteTraits,ContinuousTraits,Quantity,Organism\n");
            
            // 写入数据
            for (SequenceData seq : sequences) {
                writer.write(String.format("%d,%s,%s,%s,%s,%d,%s\n",
                    seq.getId(),
                    seq.getName(),
                    seq.getSequence(),
                    seq.getDiscreteTraits(),
                    seq.getContinuousTraits(), 
                    seq.getQuantity(),
                    seq.getOrganism()));
            }
        }
    }
} 