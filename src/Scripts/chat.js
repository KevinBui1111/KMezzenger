var chat;
var dicContact = {};
var dicMessage = {};
var lastScrollFireTime = 0, lastNotifyTime = 0;
var activeUser, top_username;
var notify_message_on = true;
var tryingToReconnect = false;

var isActive = true;

var aho_corasick;
var dic_emoticons = {};
var arr_emo = [];

window.onfocus = function () {
    isActive = true;
    if (activeUser)
        check_message_show(get_content_message(activeUser));

    if (timer_notify_new_message)
        clearInterval(timer_notify_new_message);
};

window.onblur = function () {
    isActive = false;
};
window.onclick = function (event) {
    if (!$(event.target).is('#btn_emoticon, #emoticon_list, #emoticon_list *')) {
        hide_emoticon_list();
    }
};

$(document).ready(function () {
    if (typeof Notification === "undefined") {
        alert('Desktop notifications not available in your browser. Try Chromium.');
        //return;
    }
    // request permission on page load
    else if (Notification.permission !== "granted")
        Notification.requestPermission();

    window.onbeforeunload = function () {
        $.connection.hub.stop();
    }
    $("a").click(function () {
        $.connection.hub.stop();
    });

    $('#message').keypress(function (e) {
        if (e.ctrlKey || e.altKey || e.shiftKey) { }
        else if (e.keyCode == 13) {
            e.preventDefault();
        }
    });
    $('#message').keyup(function (e) {
        if (e.ctrlKey || e.altKey || e.shiftKey) { }
        else if (e.keyCode == 13) {
            $("#sendmessage").click();
            console.log("send");
        }
    });

    $('#contact_list').on('click', '.contact', on_select_contact);
    // Reference the auto-generated proxy for the hub.  
    chat = $.connection.chatHub;
    // Create a function that the hub can call back to display messages.
    chat.client.on_receive_message = on_receive_message;
    // Create a function that the hub can call back to notify there is a new user connect.
    chat.client.on_buddy_status_changed = on_buddy_status_changed;
    // Create a function that the hub can call back to notify list of contact.
    chat.client.on_receive_contacts = on_receive_contacts;
    chat.client.on_deliveried_message = on_response_send_message;
    chat.client.on_exception_handler = on_exception_handler;

    // Get the user name and store it to prepend to messages.
    //$('#displayname').val(prompt('Enter your name:', ''));
    $('#message').focus();

    // Start the connection.
    $.connection.hub.qs = { 'username': $('#displayname').val() };
    $.connection.hub.start().done(onConnected);

    $.connection.hub.reconnecting(on_reconnecting);
    $.connection.hub.reconnected(on_reconnected);
    $.connection.hub.disconnected(function () {
        //setTimeout(function () { $.connection.hub.start(); }, 5000); // Restart connection after 5 seconds.
    });

    //load emoticons
    var emoticons_string = $.get('../Content/emoticon_list.txt', on_load_emoticons);
});
function on_load_emoticons(data) {
    var keywords = [];

    data.split('\r\n').forEach(function (line, l_index) {
        var emo = line.split('\t');
        var emo_keys = [];
        emo.forEach(function (item, index) {
            if (item && index > 1) {
                item = item.toLowerCase();
                dic_emoticons[item] = emo[1];
                keywords.push(item);
                emo_keys.push(item);
            }
        });

        arr_emo[emo[0]] = { path: emo[1], key: emo_keys };
    });

    // build #emoticon_list DOM
    var emoj_il = '';
    arr_emo.forEach(function (item, index) {
        emoj_il += "<li><img data-index='" + index + "' src='" + baseUrl + item.path + "' onmouseover='on_over_emoticon(" + index + ");' onclick='insert_emoticon(" + index + ");' /></li>\n";
    });
    $('#emoticon_list ul').html(emoj_il);

    aho_corasick = new AhoCorasick(true, keywords);
}
function insert_emoticon(index) {
    $('#message').insertAtCaret(arr_emo[index].key[0]);
}
function on_over_emoticon(index) {
    $('.key_emoticon').html(arr_emo[index].key.join(" "));
    //console.log(arr_emo[index].key);
}

function on_exception_handler(error) {
    console.log('SignalrAdapter: ' + error);
}
function on_reconnecting() {
    if (tryingToReconnect) return;

    tryingToReconnect = true;
    ShowLoading();
}
function on_reconnected() {
    tryingToReconnect = false;
    HideLoading();
}

function notifyUserOfTryingToReconnect() {
    console.log('trying to reconnect');
}

function on_receive_contacts(message) {
    //$('#contact_list .contact').remove();
    $.each(message, function (index, value) {
        on_buddy_status_changed(value);
    });
}
function on_select_contact() {
    activeUser = $('.contact.selected .name_contact').html();
    var selectedUser = $(this).find('.name_contact').html();

    if (activeUser == selectedUser) return;

    activeUser = selectedUser;
    var selectConv = get_content_message(selectedUser);

    $('.messDiv .convDiv').hide();
    selectConv.show();

    $('#contact_list .contact').removeClass('selected');
    $(this).addClass('selected');

    $('.buddy_name').html(selectedUser);
    $('#message')[0].disabled = false;

    check_message_show(selectConv);
}
function on_buddy_status_changed(buddy) {
    // Add the message to the page.
    var contact = buddy.username in dicContact ?
        dicContact[buddy.username] :
        add_contact_to_list(buddy.username);

    contact.removeClass('offline online');
    contact.addClass(buddy.status == 0 ? 'offline' : 'online');
}
function on_receive_message(message) {
    // Add the message to the page. 
    var mess = $('#templateDiv .leftFrame')[0].outerHTML.format(formatDate(new Date(message.date_sent), "hh:mm tt"), message.content);
    var div_content = get_content_message(message.from)
    div_content.append(mess);

    top_username = message.from;
    var contact = dicContact[message.from];
    $('.contact_title').after(contact);

    contact.data('unread_cnt', contact.data('unread_cnt') + 1);
    contact.find('.notify').html(contact.data('unread_cnt'));

    if (isActive) scrollBottom(div_content);

    if (!isActive) {
        notify_new_message();
        notifyMe(message.from, message.content);
    }

    message.date_received = new Date;
    chat.server.received_message(message);
}
function add_contact_to_list(username) {
    contact = $('#templateDiv .contact').clone().appendTo('#contact_list');
    dicContact[username] = contact;
    contact.find('.name_contact').html(username);

    return contact;
}
function onConnected() {
    $('#sendmessage').click(send_message);
}
function send_message() {
    var who = $('.contact.selected .name_contact').html();
    var message = $.trim($('#message').val());
    if (message == '') {
        $('#message').val('');
        return;
    }
    // Call the Send method on the hub.
    var time_sent = new Date;
    chat.server.send_message(who, message, time_sent, +time_sent)
        .done(on_response_send_message)
        .fail(on_fail_send_message);
    // Clear text box and reset focus for next comment. 
    $('#message').val('').focus();

    var mess = $($('#templateDiv .rightFrame')[0].outerHTML.format(formatDate(new Date(), "hh:mm tt"), replace_with_emo(message)));
    var div_content = get_content_message(who)
    div_content.append(mess);
    dicMessage[+time_sent] = mess;

    scrollBottom(div_content);
}
function on_response_send_message(message) {
    switch (message.status) {
        case 0: // sent
            dicMessage[message.client_message_id].find('.mess-status').html('➜');
            break;
        case 1: // received
            dicMessage[message.client_message_id].find('.mess-status').html('✔');
            break;
    }
}
function on_fail_send_message(error) {
    console.log(error);
}

function scrollBottom(e)
{
    //e.animate({ scrollTop: e.prop("scrollHeight") }, 1000);
    e.scrollTop(e.prop("scrollHeight"));
}
function get_content_message(username)
{
    var selectConv = $('.convDiv[username="' + username + '"]');
    if (selectConv.length == 0) {
        selectConv = $('#templateDiv .convDiv').clone();
        selectConv.attr('username', username);
        selectConv.hide();
        selectConv.insertBefore('.composeMessage');

        selectConv.scroll(on_sroll_message);
    }

    return selectConv;
}
function on_sroll_message() {
    var now = new Date().getTime();
    if (now - lastScrollFireTime > 2000) {
        var messDiv = $(this);
        setTimeout(function () { check_message_show(messDiv); }, 2000);
        lastScrollFireTime = now;
    }
}
function check_message_show(e) {
    var mess = e.find('.leftFrame:last');
    if (mess.length == 0) return;

    if (isScrolledIntoView(e[0], mess[0])) {
        var contact = dicContact[activeUser];
        contact.data('unread_cnt', 0);
        contact.find('.notify').html('');
    }
}
function isScrolledIntoView(container, el) {
    var elemPos = el.getBoundingClientRect();

    var isVisible = (elemPos.top >= 0) && (elemPos.bottom <= container.getBoundingClientRect().bottom);
    return isVisible;
}

var timer_notify_new_message;
var tick = false;
function notify_new_message() {
    clearInterval(timer_notify_new_message);
    timer_notify_new_message = setInterval(function () {
        document.title = 'KMezzenger' + (tick ? ' - New message from ' + top_username : '');
        tick = !tick;
    }, 500);
}


function notifyMe(username, message) {
    var now = new Date().getTime();
    if (now - lastNotifyTime < 2000 || !notify_message_on) return;

    if (Notification.permission !== "granted")
        Notification.requestPermission();
    else {
        var notification = new Notification('KMezzenger - ' + username, {
            icon: 'http://iconshow.me/media/images/Application/Modern-Flat-style-Icons/png/512/Chat.png',
            body: message
        });

        lastNotifyTime = now;

        setTimeout(function() { notification.close(); }, 4000);
    }

}
function toggle_notify() {
    $("div.notify_message").toggleClass("notify_message_off");
    notify_message_on = !notify_message_on;
}

function replace_with_emo(message) {
    var matches = aho_corasick.search(message)

    matches.sort(function (x, y) {
        var ret = x.position - y.position;
        if (ret == 0)
            ret = y.keyword.length - x.keyword.length;
        return ret;
    });

    var position = 0;
    var process_mess = '';
    for (var i = 0, m = ''; m = matches[i]; ++i) {
        var len_before = m.position - position;
        if (len_before < 0)
            continue;
        else if (len_before > 0)
        {
            var before = message.substr(position, len_before);
            // encode html, then append
            process_mess += htmlEncode(before);
        }
        // replace emo code by <img> tag
        process_mess += "<img src='" + baseUrl + dic_emoticons[m.keyword] + "'/>";
        position = m.position + m.keyword.length;
    }
    if (position < message.length)
        process_mess += message.substr(position, message.length - position);

    return process_mess;
}

function show_emoticon_list() {
    $('#emoticon_list').show();
}
function hide_emoticon_list() {
    $('#emoticon_list').hide();
}
jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if (document.selection) {
                //For browsers like Internet Explorer
                this.focus();
                var sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            }
            else if (this.selectionStart || this.selectionStart == '0') {
                //For browsers like Firefox and Webkit based
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        });
    }
});
