#!/usr/bin/env python3
"""Звуки игры (ТЗ §3.13): синтез на numpy, свой источник, без чужих сэмплов. Mono, 22 кГц, 16 бит.

Использование: python Tools/build_sounds.py
Выстрел, взрыв ×2, бустер, всплеск, клик CTA, короткий джингл победы.
"""
import wave
from pathlib import Path

import numpy

ROOT = Path(__file__).resolve().parent.parent
OUTPUT = ROOT / "Assets" / "_Game" / "Audio"
RATE = 22050
SEED = 4177


def time_axis(seconds):
    return numpy.arange(int(seconds * RATE)) / RATE


def noise(count, rng):
    return rng.uniform(-1.0, 1.0, count)


def smooth(signal, width):
    """Скользящее среднее: простой низкочастотный фильтр."""
    kernel = numpy.ones(width) / width
    return numpy.convolve(signal, kernel, mode="same")


def sine_sweep(start, end, seconds, curve=1.0):
    t = time_axis(seconds)
    frequency = start + (end - start) * (t / seconds) ** curve
    return numpy.sin(2 * numpy.pi * numpy.cumsum(frequency) / RATE)


def decay(seconds, rate):
    return numpy.exp(-time_axis(seconds) * rate)


def shot(rng):
    seconds = 0.26
    body = sine_sweep(520, 130, seconds, 0.6) * 0.55
    hiss = smooth(noise(body.size, rng), 3) * 0.6
    return (body + hiss) * decay(seconds, 13)


def explosion(rng, low, seconds):
    thump = sine_sweep(low * 2.4, low, seconds, 0.5) * decay(seconds, 9)
    rumble = smooth(noise(thump.size, rng), 14) * 2.6 * decay(seconds, 7)
    crack = smooth(noise(thump.size, rng), 2) * decay(seconds, 45) * 0.8
    return thump + rumble + crack


def booster(rng):
    seconds = 0.2
    pop = sine_sweep(320, 1150, seconds, 0.5)
    return (pop + 0.5 * numpy.sin(2 * numpy.pi * numpy.cumsum(numpy.full(pop.size, 1560.0)) / RATE)) * decay(seconds, 16)


def splash(rng):
    seconds = 0.42
    wash = smooth(noise(int(seconds * RATE), rng), 6) * 1.5
    attack = numpy.minimum(time_axis(seconds) / 0.03, 1.0)
    bubbles = sine_sweep(700, 260, seconds, 1.2) * 0.2
    return (wash + bubbles) * attack * decay(seconds, 7)


def click(rng):
    seconds = 0.12
    return (sine_sweep(880, 1320, seconds) + 0.4 * sine_sweep(1760, 2640, seconds)) * decay(seconds, 22)


def victory(rng):
    """Короткий мажорный подъём: пять нот и общий аккорд в конце."""
    notes = ((523.25, 0.11), (659.25, 0.11), (783.99, 0.11), (1046.5, 0.16), (783.99, 0.09), (1046.5, 0.5))
    parts = []
    for index, (frequency, seconds) in enumerate(notes):
        t = time_axis(seconds)
        tone = numpy.sin(2 * numpy.pi * frequency * t) + 0.35 * numpy.sin(4 * numpy.pi * frequency * t)
        if index == len(notes) - 1:
            tone += 0.7 * numpy.sin(2 * numpy.pi * 659.25 * t) + 0.7 * numpy.sin(2 * numpy.pi * 783.99 * t)
        parts.append(tone * numpy.minimum(t / 0.008, 1.0) * numpy.exp(-t * (3.5 if index == len(notes) - 1 else 7)))
    return numpy.concatenate(parts)


def save(name, signal):
    peak = numpy.abs(signal).max()
    fade = numpy.minimum((signal.size - numpy.arange(signal.size)) / (RATE * 0.01), 1.0)
    samples = (signal / peak * fade * 0.9 * 32767).astype(numpy.int16)
    path = OUTPUT / f"{name}.wav"
    with wave.open(str(path), "wb") as target:
        target.setnchannels(1)
        target.setsampwidth(2)
        target.setframerate(RATE)
        target.writeframes(samples.tobytes())
    return path.stat().st_size


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    rng = numpy.random.default_rng(SEED)
    sounds = {
        "Shot": shot(rng),
        "ExplosionA": explosion(rng, 62.0, 0.55),
        "ExplosionB": explosion(rng, 84.0, 0.42),
        "Booster": booster(rng),
        "Splash": splash(rng),
        "Victory": victory(rng),
        "Click": click(rng),
    }
    total = sum(save(name, signal) for name, signal in sounds.items())
    print(f"{len(sounds)} clips, {total // 1024} KB of WAV")


if __name__ == "__main__":
    main()
