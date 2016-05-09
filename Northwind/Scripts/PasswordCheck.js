$(function () {
    // Default Colors
    $("#chars").css('color', 'red');
    $("#capital").css('color', 'red');
    $("#number").css('color', 'red');
    $("#match").css('color', 'red');

    $("#password").keyup(function () {
        checkAndUpdate();
    });

    $("#password-verify").keyup(function () {
        checkAndUpdate();
    });

    
    
});

function checkAndUpdate() {
    var chars = false;
    var capital = false;
    var number = false;
    var match = false;


    var Pass = $("#password").val();
    var PassVerify = $("#password-verify").val();
    if (checkChars(Pass)) {
        chars = true;
        $("#chars").css('color', 'green');
    } else {
        chars = false;
        $("#chars").css('color', 'red');
    }

    if (checkCapital(Pass)) {
        capital = true;
        $("#capital").css('color', 'green');
    } else {
        capital = false;
        $("#capital").css('color', 'red');
    }

    if (checkNumber(Pass)) {
        number = true;
        $("#number").css('color', 'green');
    } else {
        number = false;
        $("#number").css('color', 'red');
    }

    if (checkMatch(Pass, PassVerify)) {
        match = true;
        $("#match").css('color', 'green');
    } else {
        match = false;
        $("#match").css('color', 'red');
    }
}

function checkChars(text) {
    var Length = text.length;
    if (Length >= 8) {
        return true;
    } else {
        return false;
    }
}

function checkCapital(text) {
    var regex = ".*[A-Z]+.*"

    if (regex.text(text)) {
        return true;
    } else {
        return false;
    }
}

function checkNumber(text) {
    if (/\d/.test(text)) {
        return true;
    } else {
        return false;
    }
}

function checkMatch(text1, text2) {
    if (text1 === text2) {
        return true;
    } else {
        return false;
    }
}