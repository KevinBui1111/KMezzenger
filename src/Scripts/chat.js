var chat;
var dicContact = {};
var lastScrollFireTime = 0, lastNotifyTime = 0;
var activeUser, top_username;

var isActive;
window.onfocus = function () {
    isActive = true;
    if (timer_notify_new_message)
        clearInterval(timer_notify_new_message);
};

window.onblur = function () {
    isActive = false;
};

$(document).ready(function () {
    // request permission on page load
    if (Notification.permission !== "granted")
        Notification.requestPermission();

    if (!Notification) {
        alert('Desktop notifications not available in your browser. Try Chromium.'); 
        return;
    }

    $('#message').keyup(function (e) {
        if (e.ctrlKey || e.altKey || e.shiftKey) { }
        else if (e.keyCode == 13) {
            $("#sendmessage").click();
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
    chat.client.on_result_send_message = on_result_send_message;

    // Get the user name and store it to prepend to messages.
    //$('#displayname').val(prompt('Enter your name:', ''));
    $('#message').focus();

    // Start the connection.
    $.connection.hub.qs = { 'username': $('#displayname').val() };
    $.connection.hub.start().done(onConnected);

    var tryingToReconnect = false;
    $.connection.hub.reconnecting(function () { tryingToReconnect = true; });
    $.connection.hub.reconnected(function () { tryingToReconnect = false; });
    $.connection.hub.disconnected(function () {
        setTimeout(function () { $.connection.hub.start(); }, 5000); // Restart connection after 5 seconds.
    });
});
function notifyUserOfTryingToReconnect() {
    console.log('trying to reconnect');
}

function on_result_send_message(message_id, result, error) {
            
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
    var mess = $('#templateDiv .leftFrame')[0].outerHTML.format(formatDate(new Date(message.date_sent), "hh:mm tt"), htmlEncode(message.content));
    var div_content = get_content_message(message.from)
    div_content.append(mess);

    top_username = message.from;
    var contact = dicContact[message.from];
    $('.contact_title').after(contact);

    contact.data('unread_cnt', contact.data('unread_cnt') + 1);
    contact.find('.notify').html(contact.data('unread_cnt'));

    scrollBottom(div_content);

    if (!isActive) {
        notify_new_message();
        notifyMe(message.from, message.content);
    }
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
    chat.server.send_message(who, message, new Date(), 1);
    // Clear text box and reset focus for next comment. 
    $('#message').val('').focus();

    var mess = $('#templateDiv .rightFrame')[0].outerHTML.format(formatDate(new Date(), "hh:mm tt"), htmlEncode(message));
    var div_content = get_content_message(who)
    div_content.append(mess);

    scrollBottom(div_content);
}
// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
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
    if (now - lastNotifyTime < 2000) return;

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
