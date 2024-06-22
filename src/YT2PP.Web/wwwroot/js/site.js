// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(document).ajaxStart(function () {
    $("#loading-spinner").show();
}).ajaxStop(function () {
    $("#loading-spinner").hide();
});


function getYouTubeVideoId(url) {
    // Regex pattern to find YouTube video ID
    var regExp = /^(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})/;
    var match = url.match(regExp);

    if (match && match[1].length === 11) {
        return match[1];
    } else {
        return null;
    }
}