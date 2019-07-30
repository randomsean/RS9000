const elements = {
    radar: document.getElementById('radar'),
    control: document.getElementById('control'),
    speed: document.getElementById('patrol-speed'),
    antennas: {
        front: {
            speed: document.getElementById('front-speed'),
            fast: document.getElementById('front-fast'),
            lamps: {
                same: document.getElementById('front-same'),
                opp: document.getElementById('front-opp'),
                xmit: document.getElementById('front-xmit'),
                fwd: document.getElementById('front-fwd'),
                bwd: document.getElementById('front-bwd'),
            },
        },
        rear: {
            speed: document.getElementById('rear-speed'),
            fast: document.getElementById('rear-fast'),
            lamps: {
                same: document.getElementById('rear-same'),
                opp: document.getElementById('rear-opp'),
                xmit: document.getElementById('rear-xmit'),
                fwd: document.getElementById('rear-fwd'),
                bwd: document.getElementById('rear-bwd'),
            },
        },
    },
    plateDisplay: document.getElementById('plates'),
    plateReader: {
        front: {
            plate: document.getElementById('front-plate'),
            lamps: {
                power: document.getElementById('front-plate-power'),
                lock: document.getElementById('front-plate-lock'),
            },
        },
        rear: {
            plate: document.getElementById('rear-plate'),
            lamps: {
                power: document.getElementById('rear-plate-power'),
                lock: document.getElementById('rear-plate-lock'),
            },
        },
    },
};

const controls = {
    radarPower: document.getElementById('radar-power'),
    radarDisplay: document.getElementById('radar-display'),
    radarBeep: document.getElementById('radar-beep'),
    antennas: {
        front: {
            power: document.getElementById('front-power'),
            mode: document.getElementById('front-mode'),
        },
        rear: {
            power: document.getElementById('rear-power'),
            mode: document.getElementById('rear-mode'),
        },
    },
};

var settings = {
    resourceName: 'rs9000',
    plateReader: false,
}

const messageTypes = {
    initialize: 0,
    heartbeat: 1,
    switchMode: 2,
    displayRadar: 3,
    displayControl: 4,
    radarPower: 5,
    antennaPower: 6,
    radarBeep: 7,
    targetLock: 8,
};

const antennaModes = {
    same: 0,
    opp: 1,
};

const targetDirections = {
    none: 0,
    coming: 1,
    going: 2,
};

function clearDisplays(value, antennas) {
    if (value === undefined) {
        value = '';
    }
    elements.speed.innerText = formatDisplay(value);
    for (let antenna in elements.antennas) {
        if (antennas != undefined && !antennas.includes(antenna)) {
            continue;
        }
        elements.antennas[antenna].speed.innerText = formatDisplay(value);
        elements.antennas[antenna].fast.innerText = formatDisplay(value);
        if (settings.plateReader) {
            elements.plateReader[antenna].plate.innerText = '';
        }
    }
}

function clearLamps(antennaNames) {
    let antennas = elements.antennas;
    for (let antenna in antennas) {
        if (antennaNames != undefined && !antennaNames.includes(antenna)) {
            continue;
        }

        for (let lamp in antennas[antenna].lamps) {
            antennas[antenna].lamps[lamp].classList.remove('lit');
        }

        if (settings.plateReader) {
            for (let lamp in elements.plateReader.lamps) {
                elements.plateReader.lamps[lamp].classList.remove('lit');
            }
        }
    }
}

function setPower(powered) {
    setButtonLamp(controls.radarPower, powered);
    clearDisplays(powered ? 0 : '');
    if (!powered) {
        clearLamps();
    }
}

function setAntennaPower(antennaName, powered, mode) {
    clearLamps([antennaName]);
    setLamp(elements.antennas, antennaName, 'xmit', powered);
    if (settings.plateReader) {
        setLamp(elements.plateReader, antennaName, 'power', powered);
    }
    if (powered) {
        switchMode(antennaName, mode);
    }
    clearDisplays(powered ? 0 : '', [antennaName]);

    setButtonLamp(controls.antennas[antennaName].power, powered);
}

function setAntennaDirection(antennas) {
    for (let i in antennas) {
        let a = antennas[i];
        setLamp(elements.antennas, a.name, 'fwd', a.dir == targetDirections.going);
        setLamp(elements.antennas, a.name, 'bwd', a.dir == targetDirections.coming);
    }
}

function init(data) {
    if (data.resourceName !== undefined) {
        settings.resourceName = data.resourceName;
    }
    settings.plateReader = data.plateReader === undefined ? false : data.plateReader;
}

function formatDisplay(n) {
    // NOTE: the 7-segment font we're using uses '!' as the empty digit character
    var v = n.toString();
    var s = '';
    for (var i = 0; i < 3 - v.length; i++) {
        s += '!';
    }
    return s + v;
}

function setDisplays(speed, antennas) {
    elements.speed.innerText = formatDisplay(speed);
    antennas.forEach(function (a) {
        var els = elements.antennas[a.name];
        if (els != null) {
            els.speed.innerText = formatDisplay(a.speed);
            els.fast.innerText = formatDisplay(a.fast);
        }
    });
}

function heartbeat(data) {
    setDisplays(data.speed, data.antennas);
    setAntennaDirection(data.antennas);
}

function switchMode(antennaName, mode) {
    setLamp(elements.antennas, antennaName, 'same', mode == antennaModes.same);
    setLamp(elements.antennas, antennaName, 'opp', mode == antennaModes.opp);
}

function setButtonLamp(el, enabled) {
    if (enabled) {
        el.classList.add('lit');
    } else {
        el.classList.remove('lit');
    }
}

function setLamp(antennas, antennaName, lampName, enabled) {
    var el = antennas[antennaName].lamps[lampName];
    if (el == null) {
        return;
    }
    if (enabled) {
        el.classList.add('lit');
    } else {
        el.classList.remove('lit');
    }
}

function setPlateDisplay(antennaName, plate) {
    let value = plate;
    if (value === undefined) {
        value = '';
    }
    elements.plateReader[antennaName].plate.innerText = value;
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

function sendMessage(action, data) {
    let body = null;
    if (data != null) {
        body = { data: data };
    }
    $.post('http://' + settings.resourceName + '/' + action, JSON.stringify(body), function (response) {
        console.log(response);
    });
}

window.addEventListener('message', function(e) {
    var item = e.data;

    switch (item.type) {
        case messageTypes.initialize:
            init(item.data);
            break;
        case messageTypes.heartbeat:
            heartbeat(item.data);
            break;
        case messageTypes.switchMode:
            switchMode(item.data.name, item.data.mode);
            break;
        case messageTypes.displayRadar:
            setDisplay(elements.radar, item.data);
            if (settings.plateReader) {
                setDisplay(elements.plateDisplay, item.data)
            }
            break;
        case messageTypes.displayControl:
            setDisplay(elements.control, item.data);
            break;
        case messageTypes.radarPower:
            setPower(item.data);
            break;
        case messageTypes.antennaPower:
            setAntennaPower(item.data.name, item.data.powered, item.data.mode);
            break;
        case messageTypes.radarBeep:
            setButtonLamp(controls.radarBeep, item.data);
            break;
        case messageTypes.targetLock:
            if (settings.plateReader) {
                setLamp(elements.plateReader, item.data.name, 'lock', item.data.locked);
                setPlateDisplay(item.data.name, item.data.plate);
            }
            if (!item.data.locked) {
                clearDisplays(0, [item.data.name]);
            }
            break;
    }
});

const buttons = document.getElementsByTagName('button');

for (var i = 0; i < buttons.length; i++) {
    let event = buttons[i].getAttribute('data-event');
    if (event === undefined || event == '') {
        continue;
    }
    buttons[i].onclick = function () {
        sendMessage(this.getAttribute('data-event'), this.getAttribute('data-value'));
    }
}

document.onkeyup = function (e) {
    if (e.keyCode == 27) { // Esc
        sendMessage('close', null);
    }
}