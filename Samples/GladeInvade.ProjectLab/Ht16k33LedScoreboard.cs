﻿using GladeInvade.Shared;
using Meadow.Foundation.ICs.IOExpanders;

namespace GladeInvade.ProjectLab;

public class Ht16k33LedScoreboard : IScoreBoard
{
    private readonly Ht16k33 _display;

    public Ht16k33LedScoreboard(Ht16k33 display)
    {
        _display = display;
        _display.SetDisplayOn(true);
        _display.SetBrightness(Ht16k33.Brightness.Low);
        ClearDisplay();
    }

    public void ClearDisplay()
    {
        _display.ClearDisplay();
    }

    public void SetDisplay(string characters)
    {
        ClearDisplay();
        for (var index = 0; index < 4; index++)
        {
            if (characters.Length <= index)
            {
                break;
            }
            
            DisplayCharacter(characters[index], (byte)index);
            
        }
        
        _display.UpdateDisplay();
    }

    // 0-3: top horizontal - a
    // 4-7: right middle horizontal - g2/i
    // 8: colon - d1/d2
    // 16-19: right top vertical - b
    // 20-23: left top diagonal - h
    // 24: decimal point - dp
    // 32-35: right bottom vertical - c
    // 36-39: middle top vertical - j
    // 48-51: bottom horizontal - d
    // 52-55: right top diagonal - k
    // 64-67: left bottom vertical - e
    // 68-71: right bottom diagonal - l 
    // 80-83: left top vertical - f
    // 84-87: middle bottom vertical - m
    // 96-99: left middle horizontal - g1/g
    // 100-103: left bottom diagonal - n

    private void DisplayCharacter(char character, byte slot)
    {
        if (slot >= 4)
        {
            // only 4 slots available
            return;
        }
        
        if (character < 32 || character > 126)
        {
            // Out of ascii range
            return;
        }

        var data = FourteenSegmentAscii[character - 32];
        for (var bit = 0; bit < 16; bit++)
        {
            if (((data >> bit) & 1) != 0)
            {
                var index = (byte)(SegmentIndices[bit] + slot);
                _display.SetLed(index, true);
            }
        }
    }

    private static readonly byte[] SegmentIndices =
    {
        // abcdefghijklmn
        0, 16, 32, 48, 64, 80, 96, 20, 4, 36, 52, 68, 84, 100
    };

    private static readonly ushort[] FourteenSegmentAscii =
    {
        // nmlkjihgfedcba
        0b00000000000000, // ' ' (space)
        0b00001000001000, // '!'
        0b00001000000010, // '"'
        0b01001101001110, // '#'
        0b01001101101101, // '$'
        0b10010000100100, // '%'
        0b00110011011001, // '&'
        0b00001000000000, // '''
        0b00000000111001, // '('
        0b00000000001111, // ')'
        0b11111010000000, // '*'
        0b01001101000000, // '+'
        0b10000000000000, // ','
        0b00000101000000, // '-'
        0b00000000000000, // '.'
        0b10010000000000, // '/'
        0b00000000111111, // '0'
        0b00010000000110, // '1'
        0b00000101011011, // '2'
        0b00000101001111, // '3'
        0b00000101100110, // '4'
        0b00000101101101, // '5'
        0b00000101111101, // '6'
        0b01010000000001, // '7'
        0b00000101111111, // '8'
        0b00000101100111, // '9'
        0b00000000000000, // ':'
        0b10001000000000, // ';'
        0b00110000000000, // '<'
        0b00000101001000, // '='
        0b01000010000000, // '>'
        0b01000100000011, // '?'
        0b00001100111011, // '@'
        0b00000101110111, // 'A'
        0b01001100001111, // 'B'
        0b00000000111001, // 'C'
        0b01001000001111, // 'D'
        0b00000101111001, // 'E'
        0b00000101110001, // 'F'
        0b00000100111101, // 'G'
        0b00000101110110, // 'H'
        0b01001000001001, // 'I'
        0b00000000011110, // 'J'
        0b00110001110000, // 'K'
        0b00000000111000, // 'L'
        0b00010010110110, // 'M'
        0b00100010110110, // 'N'
        0b00000000111111, // 'O'
        0b00000101110011, // 'P'
        0b00100000111111, // 'Q'
        0b00100101110011, // 'R'
        0b00000110001101, // 'S'
        0b01001000000001, // 'T'
        0b00000000111110, // 'U'
        0b10010000110000, // 'V'
        0b10100000110110, // 'W'
        0b10110010000000, // 'X'
        0b01010010000000, // 'Y'
        0b10010000001001, // 'Z'
        0b00000000111001, // '['
        0b00100010000000, // '\'
        0b00000000001111, // ']'
        0b10100000000000, // '^'
        0b00000000001000, // '_'
        0b00000010000000, // '`'
        0b00000101011111, // 'a'
        0b00100001111000, // 'b'
        0b00000101011000, // 'c'
        0b10000100001110, // 'd'
        0b00000001111001, // 'e'
        0b00000001110001, // 'f'
        0b00000110001111, // 'g'
        0b00000101110100, // 'h'
        0b01000000000000, // 'i'
        0b00000000001110, // 'j'
        0b01111000000000, // 'k'
        0b01001000000000, // 'l'
        0b01000101010100, // 'm'
        0b00100001010000, // 'n'
        0b00000101011100, // 'o'
        0b00010001110001, // 'p'
        0b00100101100011, // 'q'
        0b00000001010000, // 'r'
        0b00000110001101, // 's'
        0b00000001111000, // 't'
        0b00000000011100, // 'u'
        0b10000000010000, // 'v'
        0b10100000010100, // 'w'
        0b10110010000000, // 'x'
        0b00001100001110, // 'y'
        0b10010000001001, // 'z'
        0b10000011001001, // '{'
        0b01001000000000, // '|'
        0b00110100001001, // '}'
        0b00000101010010, // '~'
        0b11111111111111, // Unknown character (DEL or RUBOUT)
    };
}