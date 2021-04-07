load_code("SignalR");

game_log("Starting");
var lastPing = null;
class ALHub{
	constructor(url, hubname){
		this.url = url;
		this.hubname = hubname;
		this.connecting = true;
		this.Connect(url, hubname);
		
	}
	
	Connect(){
		let connection = $.hubConnection();
		connection.url = this.url;
		var hubClass = this;
		
		this.Proxy = connection.createHubProxy(this.hubname);
		
		this.Proxy.on('Pong', function (message) {
				console.log(new Date() - lastPing);
				lastPing = null;
		});
		
		this.Proxy.on('PathFound', function (path) {
			if(path.length > 0)
			{
				currentPath = path;
				//curPathIndex = 1;

				for(id in path)
				{
					var node = path[id];

					if(node.Action != "Move" || can_move_to(node.X, node.Y))
					{
						//curPathIndex = id;

						if(node.Action != "Move")
						{
							break;
						}
					}
					else
					{
						break;
					}

				}

				if(new Date() - waitDate > 100)
				{
					game_log("Warning: Path took " + (new Date() - waitDate) + " ms to find.");
				}
			}
			waiting = false;
		});
		this.Proxy.on('GetMesh', function (mesh) {
			console.log(mesh);
			clear_drawings();

			if(parent.mapMesh == null || parent.mapMesh.map != character.map)
				{
					if(parent.mapMesh != null)
					{
						try{parent.mapMesh.e.destroy({children:true})}catch(ex){}
					}

					var e=new PIXI.Graphics();
					for(var i = 0; i < mesh.length; i++)
					{
						var line = mesh[i];
						var p1 = line.P1;
						var p2 = line.P2;


						cellLine(p1.X, p1.Y, p2.X, p2.Y, 1, 0xEA1111, e);
					}
					e.endFill();
					parent.map.addChild(e); //e.destroy() would remove it, if you draw too many things and leave them there, it will likely bring the game to a halt

					parent.mapMesh = {map: character.map, e: e};
					meshPending = false;
				}
		});
		connection.start().done(function() {
			game_log("Connected");
			hubClass.connected = true;
			hubClass.connecting = false;
			hubClass.Proxy.invoke('Initialize', character.name);
			connection.disconnected(function(){
				hubClass.connected = false;
				//hubClass.connecting = false;
				game_log("Disconnected...");
				connection.stop();
				waiting = false;
			});
			connection.reconnecting(function(){
				hubClass.connected = false;
				//hubClass.connecting = false;
				game_log("Disconnected...");
				connection.stop();
				waiting = false;
			});
			
			lastPing = new Date();
		}).fail(function(){
			hubClass.connected = false;
			hubClass.connecting = false;
			game_log('Could not Connect!');
			///setTimeout(function(){hubClass.Connect();}, 2500); 
		});
		
			
	}
}

let hub = {};

setInterval(function(){
	
	if(!hub.connected && !hub.connecting)
	{
		hub = new ALHub('http://localhost:8080/signalr', 'adventurelandHub');
	}
	else if(hub.connected && !hub.initialized)
	{
		hub.Proxy.invoke('Ping');
		lastPing = new Date();
	}
	
	console.log(hub);
	console.log($.hubConnection());
	set_message(hub.connected);
}, 1000);

drawDebug = true;

var waiting;
var waitDate;

if(parent.mapMesh != null)
{
	try{parent.mapGrid.e.destroy({children:true})}catch(ex){}
	parent.mapMesh = null;
}

var meshPending;


//The below code needs to be run by the Party Leader, and anyone in the party that wishes to automatically follow through teleports.
//Clean out an pre-existing listeners
if (parent.prev_handlersignalrhub) {
    for (let [event, handler] of parent.prev_handlersignalrhub) {
      parent.socket.removeListener(event, handler);
    }
}

parent.prev_handlersignalrhub = [];

//handler pattern shamelessly stolen from JourneyOver
function register_signalrhubhandler(event, handler) 
{
	parent.prev_handlersignalrhub.push([event, handler]);
    parent.socket.on(event, handler);
};

var reportPlayers = true;
var reportMonsters = [];
function signalREntitesHandler(event)
{
	for(id in parent.entities)
	{
		var entity = parent.entities[id];
		
		var isPlayerToReport = reportPlayers && entity.type == "character" && !entity.citizen;
		var isMonsterToReport = entity.type == "monster" && reportMonsters.indexOf(entity.mtype) != -1;
		
		if(isPlayerToReport || isMonsterToReport)
		{
			var reducedEntity = {};
		}
	}
}

//Register event handlers
register_signalrhubhandler("entities", signalREntitesHandler);

//var curPathIndex = 1;
var currentPath;
var target;
var drawMesh = false;
setInterval(function(){
	followCurrentPath();
	if(hub.connected)
	{
		if(drawMesh && !meshPending && (parent.mapMesh == null || parent.mapMesh.map != character.map))
		{
			hub.Proxy.invoke('GetMesh', character.map);
			meshPending = true;
		}
		if(target || true)
		{
			goToPoint(-210, -93, "main");
		}
	}
}, 40);

setInterval(function(){
	var numTargetingMe = getNumTargetingMe();
	
	followCurrentPath();
}, 10);


function getNumTargetingMe()
{
	var count = 0;
	for(id in parent.entities)
	{
		var entity = parent.entities[id];
		
		if(entity.type == "monster")
		{
			if(entity.target == character.name)
			{
				count++;
			}
		}
	}
	
	return count;
}

function goToPoint(x, y, map)
{
	if(!waiting)
	{
		waiting = true;
		waitDate = new Date();
		var target = {X: x, Y: y};
		var pathObj = {To: {X: target.X, Y: target.Y, Map: map}, From: {X: character.real_x, Y: character.real_y, Map: character.map}};
		hub.Proxy.invoke('FindPath', JSON.stringify(pathObj));
	}
}

function getCurrentNode()
{
	if(currentPath != null)
	{
		var curIndex = 1;
		
		for(id in currentPath)
		{
			var node = currentPath[id];
			
			if(node.Action != "Move" || can_move_to(node.X, node.Y))
			{
				curIndex = id;
				
				if(node.Action != "Move")
				{
					break;
				}
			}
			else
			{
				break;
			}
			
		}
		
		return parseInt(curIndex);
	}
	
	return null;
}

var avoidMinDistance = 60;
var avoidMaxDistance = 90;
var buffer = 40;
var charDistance = 30;
function getAvoidVector()
{
	var vector = new Vector();
	for(id in parent.entities)
	{
		var entity = parent.entities[id];
		
		if(entity.mtype == "boar" || entity.type == "character" && !entity.citizen)
		{
			var minDist = charDistance;
			
			if(entity.mtype)
			{
				var range = parent.G.monsters[entity.mtype].range;
				minDist = range + buffer;
			}
			var maxDist = minDist + buffer;
			
			var dist = distance2D(entity.real_x, entity.real_y);
			
			if(dist < maxDist)
			{
				draw_circle(entity.real_x, entity.real_y, minDist);
				var entVector = new Vector(character.real_x - entity.real_x, character.real_y - entity.real_y).normalize();
				
				var scale = 1 - ((dist - minDist) / (maxDist - minDist));
				
				var scaledVector = entVector.multiply(scale * 15);

				vector = vector.add(scaledVector);
			}
		}
	}
	var normVector = vector;
	//game_log(normVector.x + "," + normVector.y);
	if(normVector.length() > 25)
	{
		normVector = normVector.normalize().multiply(25);
	}
	return normVector
}

var lastMove;
function followCurrentPath()
{
	clear_drawings();
	var moveVector = new Vector();
	var avoidVector = getAvoidVector();
	//moveVector.add(avoidVector);
	if(currentPath != null)
	{
		for(var i = 0; i < currentPath.length - 1; i++)
		{
			var p1 = currentPath[i];
			var p2 = currentPath[i+1];

			draw_line(p1.X, p1.Y, p2.X, p2.Y);
			
			if(p2.Action != "Move")
			{
				break;
			}
		}
		var curPathIndex = getCurrentNode();
		var curPoint = currentPath[curPathIndex];
		var nextNode = null;
		if(curPathIndex < currentPath.length - 1)
		{
			nextNode = currentPath[curPathIndex + 1];
		}
		
		if(nextNode != null)
		{
			var closestToCur = closestOnLine({x: curPoint.X, y: curPoint.Y}, {x: character.real_x, y: character.real_y}, {x: nextNode.X, y: nextNode.X});

			draw_line(curPoint.X, curPoint.Y, closestToCur.x, closestToCur.y);
			
			if(avoidVector.x > 0 && avoidVector.y > 0)
			{
				var offsetVector = new Vector(closestToCur.x - curPoint.X, closestToCur.y - curPoint.Y);
				if(offsetVector.length() > 2)
				{
					game_log("offsetting");
					offsetVector = offsetVector.normalize().multiply(-15);
				
					//moveVector.add(offsetVector);
				}
			}
		}
		//draw_circle(character.going_x, character.going_y, 10);
		
			var dist = distance2D(curPoint.X, curPoint.Y);
			if(lastMove == null || new Date() - lastMove > 100 || true)// && !(character.going_x == curPoint.X && character.going_y == curPoint.Y))
			{
				var vectorMag = 10;
				if(dist < 10)
				{
					vectorMag = dist;
				}

				var pathVector = new Vector(curPoint.X - character.real_x, curPoint.Y - character.real_y).normalize().multiply(vectorMag);
				moveVector.add(pathVector);
			}
			
		if(dist < 1)
		{
			if(curPoint.Action == "Transport")
			{
				if(character.map != curPoint.ActionTarget)
				{
					game_log(curPoint.ActionTarget);
					game_log(curPoint.TargetSpawn);
					
					var spawn = curPoint.TargetSpawn;
					
					if(curPoint.ActionTarget == "level2")
					{
						spawn = 4;
					}
					
					parent.transport_to(curPoint.ActionTarget, spawn);
				}
				else
				{
					curPathIndex++;

					if(curPathIndex == currentPath.length)
					{
						currentPath = null;
						target = null;
					}
				}
			}
			else
			{
				curPathIndex++;

				if(curPathIndex == currentPath.length)
				{
					currentPath = null;
					target = null;
				}
			}
		}
	}
	var lineVector = new Vector(moveVector.x, moveVector.y).normalize().multiply(50);
	draw_line(character.real_x, character.real_y, lineVector.x + character.real_x, lineVector.y + character.real_y, 1, 0xEA1111)
	if(lastMove == null || new Date() - lastMove > 50)// && !(character.going_x == curPoint.X && character.going_y == curPoint.Y))
	{
		var len = moveVector.length();
		if(len > 1)
		{
			if(len > 25)
			{
				moveVector = moveVector.normalize().multiply(25);
			}
			move(moveVector.x + character.real_x, moveVector.y + character.real_y);
			lastMove = new Date();
		}
	}
}

function closestOnLine( p, a, b ) {
    
    var atob = { x: b.x - a.x, y: b.y - a.y };
    var atop = { x: p.x - a.x, y: p.y - a.y };
    var len = atob.x * atob.x + atob.y * atob.y;
    var dot = atop.x * atob.x + atop.y * atob.y;
    var t = min( 1, max( 0, dot / len ) );

    dot = ( b.x - a.x ) * ( p.y - a.y ) - ( b.y - a.y ) * ( p.x - a.x );
    
    return {
        x: a.x + atob.x * t,
        y: a.y + atob.y * t
    };
}

function distance2D(x2, y2)
{
	var a = character.real_x - x2;
	var b = character.real_y - y2;

	var c = Math.sqrt( a*a + b*b );
	
	return c;
}
//DRAWING

function cellLine(x,y,x2,y2,size,color,e)
{
	// keep in mind that drawings could significantly slow redraws, especially if you don't .destroy() them
	if(!color) color=0xF38D00;
	if(!size) size=2;
	e.lineStyle(size, color);
	e.moveTo(x,y);
	e.lineTo(x2,y2);
}

/*
Simple 2D JavaScript Vector Class
Hacked from evanw's lightgl.js
https://github.com/evanw/lightgl.js/blob/master/src/vector.js
*/

function Vector(x, y) {
	this.x = x || 0;
	this.y = y || 0;
}

/* INSTANCE METHODS */

Vector.prototype = {
	negative: function() {
		this.x = -this.x;
		this.y = -this.y;
		return this;
	},
	add: function(v) {
		if (v instanceof Vector) {
			this.x += v.x;
			this.y += v.y;
		} else {
			this.x += v;
			this.y += v;
		}
		return this;
	},
	subtract: function(v) {
		if (v instanceof Vector) {
			this.x -= v.x;
			this.y -= v.y;
		} else {
			this.x -= v;
			this.y -= v;
		}
		return this;
	},
	multiply: function(v) {
		if (v instanceof Vector) {
			this.x *= v.x;
			this.y *= v.y;
		} else {
			this.x *= v;
			this.y *= v;
		}
		return this;
	},
	divide: function(v) {
		if (v instanceof Vector) {
			if(v.x != 0) this.x /= v.x;
			if(v.y != 0) this.y /= v.y;
		} else {
			if(v != 0) {
				this.x /= v;
				this.y /= v;
			}
		}
		return this;
	},
	equals: function(v) {
		return this.x == v.x && this.y == v.y;
	},
	dot: function(v) {
		return this.x * v.x + this.y * v.y;
	},
	cross: function(v) {
		return this.x * v.y - this.y * v.x
	},
	length: function() {
		return Math.sqrt(this.dot(this));
	},
	normalize: function() {
		return this.divide(this.length());
	},
	min: function() {
		return Math.min(this.x, this.y);
	},
	max: function() {
		return Math.max(this.x, this.y);
	},
	toAngles: function() {
		return -Math.atan2(-this.y, this.x);
	},
	angleTo: function(a) {
		return Math.acos(this.dot(a) / (this.length() * a.length()));
	},
	toArray: function(n) {
		return [this.x, this.y].slice(0, n || 2);
	},
	clone: function() {
		return new Vector(this.x, this.y);
	},
	set: function(x, y) {
		this.x = x; this.y = y;
		return this;
	}
};

/* STATIC METHODS */
Vector.negative = function(v) {
	return new Vector(-v.x, -v.y);
};
Vector.add = function(a, b) {
	if (b instanceof Vector) return new Vector(a.x + b.x, a.y + b.y);
	else return new Vector(a.x + v, a.y + v);
};
Vector.subtract = function(a, b) {
	if (b instanceof Vector) return new Vector(a.x - b.x, a.y - b.y);
	else return new Vector(a.x - v, a.y - v);
};
Vector.multiply = function(a, b) {
	if (b instanceof Vector) return new Vector(a.x * b.x, a.y * b.y);
	else return new Vector(a.x * v, a.y * v);
};
Vector.divide = function(a, b) {
	if (b instanceof Vector) return new Vector(a.x / b.x, a.y / b.y);
	else return new Vector(a.x / v, a.y / v);
};
Vector.equals = function(a, b) {
	return a.x == b.x && a.y == b.y;
};
Vector.dot = function(a, b) {
	return a.x * b.x + a.y * b.y;
};
Vector.cross = function(a, b) {
	return a.x * b.y - a.y * b.x;
};