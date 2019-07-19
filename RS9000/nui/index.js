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

window.addEventListener('message', function(e) {
    var item = e.data;

    if ('enabled' in item) {
        if (item.enabled) {
            radar.style.display = 'block';
            return;
        }
        radar.style.display = 'none';
        return;
    }

    updateSpeed(patrolSpeed, item.speed);

    item.antennae.forEach(function (antenna) {
        var obj = antennaElements[antenna.name];
        updateSpeed(obj.speed, antenna.speed);
        updateSpeed(obj.fast, antenna.fast);

        if (antenna.mode == 0) {
            setLamp(obj.lamps.same, true);
            setLamp(obj.lamps.opp, false);
        } else {
            setLamp(obj.lamps.same, false);
            setLamp(obj.lamps.opp, true);
        }
    });
});
