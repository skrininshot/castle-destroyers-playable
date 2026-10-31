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
DEFAULT_LEVEL = 0.9
# Пик клипа. Всплеск тише: слышен в районе 1 кГц, частый и накладывается сам на себя.
LEVELS = {"Splash": 0.6}


def time_axis(seconds):
    return numpy.arange(int(seconds * RATE)) / RATE


def noise(count, rng):
    return rng.uniform(-1.0, 1.0, count)


def smooth(signal, width):
    """Скользящее среднее: простой низкочастотный фильтр."""
    kernel = numpy.ones(width) / width
    return numpy.convolve(signal, kernel, mode="same")


def lowpass(signal, start, end):
    """Фильтр 12 дБ/окт со срезом, плавно падающим от start к end Гц: скользящее среднее верхи не срезает."""
    gains = 1 - numpy.exp(-2 * numpy.pi * numpy.geomspace(start, end, signal.size) / RATE)
    result = numpy.zeros(signal.size)
    first = second = 0.0
    for index, gain in enumerate(gains):
        first += gain * (signal[index] - first)
        second += gain * (first - second)
        result[index] = second
    return result


def bubble(t, start, frequency, seconds):
    """Один пузырь: тон с подъёмом высоты и мягкими краями."""
    phase = numpy.clip((t - start) / seconds, 0.0, 1.0)
    return numpy.sin(2 * numpy.pi * frequency * seconds * (phase + 0.2 * phase ** 2)) * numpy.sin(numpy.pi * phase) ** 2


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
    """Мягкий плеск: глухой удар, тёмный шум без верхов и пара пузырей. Шипение даёт верх шума, он срезан."""
    seconds = 0.5
    t = time_axis(seconds)
    wash = lowpass(noise(t.size, rng), 1300, 300) * decay(seconds, 6)
    plop = sine_sweep(230, 100, seconds, 0.7) * decay(seconds, 15) * 0.16
    bubbles = sum(bubble(t, *params) for params in ((0.06, 340, 0.09), (0.15, 460, 0.07), (0.27, 390, 0.08))) * 0.05
    attack = numpy.sin(numpy.minimum(t / 0.035, 1.0) * numpy.pi / 2) ** 2
    return (wash + plop) * attack + bubbles


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
    samples = (signal / peak * fade * LEVELS.get(name, DEFAULT_LEVEL) * 32767).astype(numpy.int16)
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
