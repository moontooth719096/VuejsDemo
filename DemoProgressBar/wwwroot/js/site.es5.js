// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
'use strict';

function getGoogleAcessToken() {
    return localStorage.getItem('googleAuthToken');
}

function getAPIGoogleAcessToken() {
    return 'Bearer ' + localStorage.getItem('googleAuthToken');
}

function setGoogleAcessToken(token) {
    localStorage.setItem('googleAuthToken', token);
}

function removeGoogleAcessToken(token) {
    localStorage.removeItem("googleAuthToken");
}

