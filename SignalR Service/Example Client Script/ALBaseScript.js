load_code("SignalR");

game_log("Starting");

let lastPing = null;

class ALHub {
    constructor(url, hubname) {
        this.url = url;
        this.hubname = hubname;
        this.connecting = true;
        this.Connect(url, hubname);
    }

    Connect() {
        let connection = $.hubConnection();
        connection.url = this.url;
        let hubClass = this;

        this.Proxy = connection.createHubProxy(this.hubname);

        this.Proxy.on('Pong', function (message) {
            set_message("Ping: " + (new Date() - lastPing));
            lastPing = null;
        });

        connection.start().done(function () {
            game_log("Connected");
            hubClass.connected = true;
            hubClass.connecting = false;
            hubClass.Proxy.invoke('Initialize', character.name);
            connection.disconnected(function () {
                hubClass.connected = false;
                game_log("Disconnected...");
                connection.stop();
            });
            connection.reconnecting(function () {
                hubClass.connected = false;
                game_log("Disconnected...");
                connection.stop();
            });
        }).fail(function () {
            hubClass.connected = false;
            hubClass.connecting = false;
            game_log('Could not Connect!');
        });
    }
}

let hub = {};

setInterval(function () {

    if (!hub.connected && !hub.connecting) {
        hub = new ALHub('http://localhost:8080/signalr', 'adventurelandHub');
    }
    else if (hub.connected) {
        hub.Proxy.invoke('Ping');
        lastPing = new Date();
    }

    console.log(hub);
    console.log($.hubConnection());
    set_message(hub.connected);
}, 1000);
