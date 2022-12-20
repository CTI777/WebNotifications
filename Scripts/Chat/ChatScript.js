$(document).ready(function () {
    ShowLogin()
});



var chatapi = '/api/chat/';
var user = null;
var source = null;

function getApiUrl(action) {
    return chatapi + (action ? action : '');
}

function getUrlLogin() {
    return getApiUrl('login');
}

function getUrlLogout() {
    if (user == null) return getApiUrl();
    return getApiUrl('logout?sessionID=' + user.sessionID);
}

function getUrlPush() {
    return getApiUrl('push');
}

function getUrlSubscribe() {
    if (user == null) return getApiUrl();
    return getApiUrl('subscribe/' + user.sessionID); 
}

function LoginOk(data, textStatus, xhr) {
    console.log('LoginOk: user: ' + data.name+" "+data.sessionID+ " textStatus=" + textStatus);
    user = data;
    Subscribe();

}

function LoginError(xhr, textStatus, errorThrown) {
    console.log('SubscribeError: textStatus=' + textStatus + " errorThrown=" + errorThrown);
}

function Login() {
    source = null;
    user = null;
    var username = document.getElementById("username").value;
    if (username == "" || username == undefined) {
        alert('please enter username');
        return;
    }
    else {
        console.log('Login ' + username);

        $.ajax({
            type: 'POST',
            url: getUrlLogin(),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ name: username, sessionId: ''}),
            success: LoginOk,
            error: LoginError
        });

    }
}
function resetSource() {
    if (source != null) source = source.close();
    source = null;
}


function Logout() {

    resetSource();

    console.log('Logout ' + (user ? (user.name + ' '+ user.sessionID) : ''));

    if (user != null) {
        $.ajax({
            type: 'POST',
            url: getUrlLogout(),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(user)
        });
    }
    ShowLogin();
  
}
function ShowLogin() {
    $('#start').show();
    $('#chatContainer').hide();
    $('#chatusers').hide();

    resetSource();
    user = null;
}

function Subscribe() {
    if (user == null) return;

    if (window.EventSource == undefined) {
        // If not supported  
        console.log("Your browser doesn't support Server Sent Events.");
        return;
    } else {

        if (source==null) {

            var url = getUrlSubscribe();

            console.log("Subscribe url=" + url+ " sessionID=" + user.sessionID);

            source = new EventSource(getUrlSubscribe(url));

            $('#start').hide();
            $('#chatContainer').show();
            $('#chatusers').show();
            $('#chatTemplate').empty();
            $('#message').text("Welcome to Chat :" + user.name).css("font-weight", "Bold");


            source.addEventListener('message', function (e) {
                ShowMessage(e);

            }, false);

            source.addEventListener('open', function (e) {
                console.log("open!");

            }, false);

            source.addEventListener('error', function (e) {
                console.log('error: e.readyState=' + e.readyState);

                if (e.readyState == EventSource.CLOSED) {
                    console.log('error: Connection Closed.');

                    ShowLogin();
                }
            }, false);

            //GetUsers();

        } 
    }
}

function Push() {

    var pushMessage = {
        sessionID: user.sessionID,
        text: document.getElementById("push").value,
    };

    $.ajax({
        type: 'POST',
        url: getUrlPush(),
        data: JSON.stringify(pushMessage),
        cache: false,
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        error: function error(xhr, textStatus, errorThrown) {
            console.log('Push Error: textStatus=' + textStatus + " errorThrown=" + errorThrown);
            ShowLogin();
        }
    });
    $("#push").val('');
}


function IsSystemMsg(n) {
    if (!n) return false;
    return (n == "$SYSTEM_NEW_USER" || n== "$SYSTEM_REM_USER");
}

function ShowMessage(e) {
    if (!e) return;
    var data = e.data.split('|');

    
    var user = $("<strong></strong>").text(data[0]);
    var dt = $("<i></i>").text(" " + data[2]);
    var txt = $("<div></div>").text(data[1]);
    var chatTemp = document.createElement("p");
    chatTemp.append(user[0], dt[0], txt[0]);
    $('#chatTemplate').prepend(chatTemp);

    if (IsSystemMsg(data[0])) setTimeout(GetUsers, 1000);
}

function GetUsers() {
    $('#users').empty();

    $.ajax({
        type: 'GET',
        url: getApiUrl('users'),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function Ok(data, textStatus, xhr) {
                console.log('Users Ok: users_cnt=' + data.length);
                $('#users').empty();

                data.forEach((user, i) => {
                    var dt = $("<div></div>").text(data[2]);


                    var username = $("<strong></strong>").text(user.name + " ("+user.sessionID+"): ");
                    var dt = $("<i></i>").text(user.dt);
                    var userTemp = document.createElement("p");
                    userTemp.append(username[0], dt[0]);
                    $('#users').append(userTemp);
                });


                 },
        error: function Error(xhr, textStatus, errorThrown) {
                    console.log('Users Error: textStatus=' + textStatus + " errorThrown=" + errorThrown);
                    ShowLogin();
               }
    });

}