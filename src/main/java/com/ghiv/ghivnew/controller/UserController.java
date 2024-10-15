package com.ghiv.ghivnew.controller;

import org.springframework.web.bind.annotation.GetMapping;

public class UserController {
    @GetMapping("/login")
    public void login() {
        //login
    }

    @GetMapping("/register")
    public void register() {
        //register
    }

}
