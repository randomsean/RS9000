var radar = document.getElementById('radar');

var antennaElements = {
    front: {
        speed: document.getElementById('front-speed'),
        fast: document.getElementById('front-fast'),
        lamps: {
            same: document.getElementById('front-same'),
            opp: document.getElementById('front-opp'),
            xmit: document.getElementById('front-xmit'),
        },
    },
    rear: {
        speed: document.getElementById('rear-speed'),
        fast: document.getElementById('rear-fast'),
        lamps: {
            same: document.getElementById('rear-same'),
            opp: document.getElementById('rear-opp'),
            xmit: document.getElementById('rear-xmit'),
        },
    }
}

const messageTypes = {
    initialize: 0,
    heartbeat: 1,
    switchMode: 2,
    displayRadar: 3,
};

var patrolSpeed = document.getElementById('patrol-speed');

function toMPH(speed) {
    // TODO: add metric support
    return speed * 2.237;
}

function leftPad(speed) {
    // NOTE: since the 7-segment font we're using is not monospace for spaces, add 4 nbsp's for each empty character.
    var v = speed.toString();
    var s = '';
    for (var i = 0; i < (3 - v.length) * 4; i++) {
        s += '\u00A0';
    }
    return s + v;
}

function updateSpeed(el, value) {
    el.innerText = leftPad(Math.round(toMPH(value)));
}

function setLamp(el, value) {
    if (value) {
        el.classList.add('lit');
    } else {
        el.classList.remove('lit');
    }
}

function heartbeat(data) {
    updateSpeed(patrolSpeed, data.patrol);

    data.antennas.forEach(function (a) {
        var el = antennaElements[a.name];
        if (el != null) {
            updateSpeed(el.speed, a.speed);
            updateSpeed(el.fast, a.fast);
        }
    });
}

function setDisplay(el, display) {
    if (el == null) {
        return;
    }
    if (display) {
        el.style.display = 'block';
    } else {
        el.style.display = 'none';
    }
}

window.addEventListener('message', function(e) {
    var item = e.data;

    switch (item.type) {
        case messageTypes.heartbeat:
            heartbeat(item.data);
            break;
        case messageTypes.displayRadar:
            setDisplay(radar, item.data);
            break;
    }
});
