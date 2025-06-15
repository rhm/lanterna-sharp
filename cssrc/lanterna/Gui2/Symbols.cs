/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */

namespace Lanterna.Gui2;

public static class Symbols
{
    /// <summary>
    /// Some text graphics, taken from http://en.wikipedia.org/wiki/Codepage_437 but converted to its UTF-8 counterpart.
    /// This class is mostly here to help out with building text GUIs when you don't have a handy Unicode chart available.
    /// Previously this class was known as ACS, which was taken from ncurses (meaning "Alternative Character Set").
    /// </summary>
    
    /// <summary>
    /// ☺
    /// </summary>
    public const char FACE_WHITE = (char)0x263A;
    /// <summary>
    /// ☻
    /// </summary>
    public const char FACE_BLACK = (char)0x263B;
    /// <summary>
    /// ♥
    /// </summary>
    public const char HEART = (char)0x2665;
    /// <summary>
    /// ♣
    /// </summary>
    public const char CLUB = (char)0x2663;
    /// <summary>
    /// ♦
    /// </summary>
    public const char DIAMOND = (char)0x2666;
    /// <summary>
    /// ♠
    /// </summary>
    public const char SPADES = (char)0x2660;
    /// <summary>
    /// •
    /// </summary>
    public const char BULLET = (char)0x2022;
    /// <summary>
    /// ◘
    /// </summary>
    public const char INVERSE_BULLET = (char)0x25d8;
    /// <summary>
    /// ○
    /// </summary>
    public const char WHITE_CIRCLE = (char)0x25cb;
    /// <summary>
    /// ◙
    /// </summary>
    public const char INVERSE_WHITE_CIRCLE = (char)0x25d9;

    /// <summary>
    /// ■
    /// </summary>
    public const char SOLID_SQUARE = (char)0x25A0;
    /// <summary>
    /// ▪
    /// </summary>
    public const char SOLID_SQUARE_SMALL = (char)0x25AA;
    /// <summary>
    /// □
    /// </summary>
    public const char OUTLINED_SQUARE = (char)0x25A1;
    /// <summary>
    /// ▫
    /// </summary>
    public const char OUTLINED_SQUARE_SMALL = (char)0x25AB;

    /// <summary>
    /// ♀
    /// </summary>
    public const char FEMALE = (char)0x2640;
    /// <summary>
    /// ♂
    /// </summary>
    public const char MALE = (char)0x2642;

    /// <summary>
    /// ↑
    /// </summary>
    public const char ARROW_UP = (char)0x2191;
    /// <summary>
    /// ↓
    /// </summary>
    public const char ARROW_DOWN = (char)0x2193;
    /// <summary>
    /// →
    /// </summary>
    public const char ARROW_RIGHT = (char)0x2192;
    /// <summary>
    /// ←
    /// </summary>
    public const char ARROW_LEFT = (char)0x2190;

    /// <summary>
    /// █
    /// </summary>
    public const char BLOCK_SOLID = (char)0x2588;
    /// <summary>
    /// ▓
    /// </summary>
    public const char BLOCK_DENSE = (char)0x2593;
    /// <summary>
    /// ▒
    /// </summary>
    public const char BLOCK_MIDDLE = (char)0x2592;
    /// <summary>
    /// ░
    /// </summary>
    public const char BLOCK_SPARSE = (char)0x2591;

    /// <summary>
    /// ►
    /// </summary>
    public const char TRIANGLE_RIGHT_POINTING_BLACK = (char)0x25BA;
    /// <summary>
    /// ◄
    /// </summary>
    public const char TRIANGLE_LEFT_POINTING_BLACK = (char)0x25C4;
    /// <summary>
    /// ▲
    /// </summary>
    public const char TRIANGLE_UP_POINTING_BLACK = (char)0x25B2;
    /// <summary>
    /// ▼
    /// </summary>
    public const char TRIANGLE_DOWN_POINTING_BLACK = (char)0x25BC;

    /// <summary>
    /// ⏴
    /// </summary>
    public const char TRIANGLE_RIGHT_POINTING_MEDIUM_BLACK = (char)0x23F4;
    /// <summary>
    /// ⏵
    /// </summary>
    public const char TRIANGLE_LEFT_POINTING_MEDIUM_BLACK = (char)0x23F5;
    /// <summary>
    /// ⏶
    /// </summary>
    public const char TRIANGLE_UP_POINTING_MEDIUM_BLACK = (char)0x23F6;
    /// <summary>
    /// ⏷
    /// </summary>
    public const char TRIANGLE_DOWN_POINTING_MEDIUM_BLACK = (char)0x23F7;

    /// <summary>
    /// ─
    /// </summary>
    public const char SINGLE_LINE_HORIZONTAL = (char)0x2500;
    /// <summary>
    /// ━
    /// </summary>
    public const char BOLD_SINGLE_LINE_HORIZONTAL = (char)0x2501;
    /// <summary>
    /// ╾
    /// </summary>
    public const char BOLD_TO_NORMAL_SINGLE_LINE_HORIZONTAL = (char)0x257E;
    /// <summary>
    /// ╼
    /// </summary>
    public const char BOLD_FROM_NORMAL_SINGLE_LINE_HORIZONTAL = (char)0x257C;
    /// <summary>
    /// ═
    /// </summary>
    public const char DOUBLE_LINE_HORIZONTAL = (char)0x2550;
    /// <summary>
    /// │
    /// </summary>
    public const char SINGLE_LINE_VERTICAL = (char)0x2502;
    /// <summary>
    /// ┃
    /// </summary>
    public const char BOLD_SINGLE_LINE_VERTICAL = (char)0x2503;
    /// <summary>
    /// ╿
    /// </summary>
    public const char BOLD_TO_NORMAL_SINGLE_LINE_VERTICAL = (char)0x257F;
    /// <summary>
    /// ╽
    /// </summary>
    public const char BOLD_FROM_NORMAL_SINGLE_LINE_VERTICAL = (char)0x257D;
    /// <summary>
    /// ║
    /// </summary>
    public const char DOUBLE_LINE_VERTICAL = (char)0x2551;

    /// <summary>
    /// ┌
    /// </summary>
    public const char SINGLE_LINE_TOP_LEFT_CORNER = (char)0x250C;
    /// <summary>
    /// ╔
    /// </summary>
    public const char DOUBLE_LINE_TOP_LEFT_CORNER = (char)0x2554;
    /// <summary>
    /// ┐
    /// </summary>
    public const char SINGLE_LINE_TOP_RIGHT_CORNER = (char)0x2510;
    /// <summary>
    /// ╗
    /// </summary>
    public const char DOUBLE_LINE_TOP_RIGHT_CORNER = (char)0x2557;

    /// <summary>
    /// └
    /// </summary>
    public const char SINGLE_LINE_BOTTOM_LEFT_CORNER = (char)0x2514;
    /// <summary>
    /// ╚
    /// </summary>
    public const char DOUBLE_LINE_BOTTOM_LEFT_CORNER = (char)0x255A;
    /// <summary>
    /// ┘
    /// </summary>
    public const char SINGLE_LINE_BOTTOM_RIGHT_CORNER = (char)0x2518;
    /// <summary>
    /// ╝
    /// </summary>
    public const char DOUBLE_LINE_BOTTOM_RIGHT_CORNER = (char)0x255D;

    /// <summary>
    /// ┼
    /// </summary>
    public const char SINGLE_LINE_CROSS = (char)0x253C;
    /// <summary>
    /// ╬
    /// </summary>
    public const char DOUBLE_LINE_CROSS = (char)0x256C;
    /// <summary>
    /// ╪
    /// </summary>
    public const char DOUBLE_LINE_HORIZONTAL_SINGLE_LINE_CROSS = (char)0x256A;
    /// <summary>
    /// ╫
    /// </summary>
    public const char DOUBLE_LINE_VERTICAL_SINGLE_LINE_CROSS = (char)0x256B;

    /// <summary>
    /// ┴
    /// </summary>
    public const char SINGLE_LINE_T_UP = (char)0x2534;
    /// <summary>
    /// ┬
    /// </summary>
    public const char SINGLE_LINE_T_DOWN = (char)0x252C;
    /// <summary>
    /// ├
    /// </summary>
    public const char SINGLE_LINE_T_RIGHT = (char)0x251c;
    /// <summary>
    /// ┤
    /// </summary>
    public const char SINGLE_LINE_T_LEFT = (char)0x2524;

    /// <summary>
    /// ╨
    /// </summary>
    public const char SINGLE_LINE_T_DOUBLE_UP = (char)0x2568;
    /// <summary>
    /// ╥
    /// </summary>
    public const char SINGLE_LINE_T_DOUBLE_DOWN = (char)0x2565;
    /// <summary>
    /// ╞
    /// </summary>
    public const char SINGLE_LINE_T_DOUBLE_RIGHT = (char)0x255E;
    /// <summary>
    /// ╡
    /// </summary>
    public const char SINGLE_LINE_T_DOUBLE_LEFT = (char)0x2561;

    /// <summary>
    /// ╩
    /// </summary>
    public const char DOUBLE_LINE_T_UP = (char)0x2569;
    /// <summary>
    /// ╦
    /// </summary>
    public const char DOUBLE_LINE_T_DOWN = (char)0x2566;
    /// <summary>
    /// ╠
    /// </summary>
    public const char DOUBLE_LINE_T_RIGHT = (char)0x2560;
    /// <summary>
    /// ╣
    /// </summary>
    public const char DOUBLE_LINE_T_LEFT = (char)0x2563;

    /// <summary>
    /// ╧
    /// </summary>
    public const char DOUBLE_LINE_T_SINGLE_UP = (char)0x2567;
    /// <summary>
    /// ╤
    /// </summary>
    public const char DOUBLE_LINE_T_SINGLE_DOWN = (char)0x2564;
    /// <summary>
    /// ╟
    /// </summary>
    public const char DOUBLE_LINE_T_SINGLE_RIGHT = (char)0x255F;
    /// <summary>
    /// ╢
    /// </summary>
    public const char DOUBLE_LINE_T_SINGLE_LEFT = (char)0x2562;

    // Legacy names for backward compatibility with existing C# code
    public const char SingleLineHorizontal = SINGLE_LINE_HORIZONTAL;
    public const char SingleLineVertical = SINGLE_LINE_VERTICAL;
    public const char SingleLineTopLeftCorner = SINGLE_LINE_TOP_LEFT_CORNER;
    public const char SingleLineTopRightCorner = SINGLE_LINE_TOP_RIGHT_CORNER;
    public const char SingleLineBottomLeftCorner = SINGLE_LINE_BOTTOM_LEFT_CORNER;
    public const char SingleLineBottomRightCorner = SINGLE_LINE_BOTTOM_RIGHT_CORNER;
    public const char SingleLineCross = SINGLE_LINE_CROSS;
    public const char SingleLineTUp = SINGLE_LINE_T_UP;
    public const char SingleLineTDown = SINGLE_LINE_T_DOWN;
    public const char SingleLineTLeft = SINGLE_LINE_T_LEFT;
    public const char SingleLineTRight = SINGLE_LINE_T_RIGHT;
    
    public const char DoubleLineHorizontal = DOUBLE_LINE_HORIZONTAL;
    public const char DoubleLineVertical = DOUBLE_LINE_VERTICAL;
    public const char DoubleLineTopLeftCorner = DOUBLE_LINE_TOP_LEFT_CORNER;
    public const char DoubleLineTopRightCorner = DOUBLE_LINE_TOP_RIGHT_CORNER;
    public const char DoubleLineBottomLeftCorner = DOUBLE_LINE_BOTTOM_LEFT_CORNER;
    public const char DoubleLineBottomRightCorner = DOUBLE_LINE_BOTTOM_RIGHT_CORNER;
    public const char DoubleLineCross = DOUBLE_LINE_CROSS;
    public const char DoubleLineTUp = DOUBLE_LINE_T_UP;
    public const char DoubleLineTDown = DOUBLE_LINE_T_DOWN;
    public const char DoubleLineTLeft = DOUBLE_LINE_T_LEFT;
    public const char DoubleLineTRight = DOUBLE_LINE_T_RIGHT;
    
    public const char DoubleLineHorizontalSingleLineCross = DOUBLE_LINE_HORIZONTAL_SINGLE_LINE_CROSS;
    public const char DoubleLineVerticalSingleLineCross = DOUBLE_LINE_VERTICAL_SINGLE_LINE_CROSS;
    
    public const char BoldSingleLineHorizontal = BOLD_SINGLE_LINE_HORIZONTAL;
    public const char BoldSingleLineVertical = BOLD_SINGLE_LINE_VERTICAL;
    public const char BoldFromNormalSingleLineHorizontal = BOLD_FROM_NORMAL_SINGLE_LINE_HORIZONTAL;
    public const char BoldFromNormalSingleLineVertical = BOLD_FROM_NORMAL_SINGLE_LINE_VERTICAL;
    public const char BoldToNormalSingleLineHorizontal = BOLD_TO_NORMAL_SINGLE_LINE_HORIZONTAL;
    public const char BoldToNormalSingleLineVertical = BOLD_TO_NORMAL_SINGLE_LINE_VERTICAL;
    
    public const char Triangle_Up_Pointing_Black = TRIANGLE_UP_POINTING_BLACK;
    public const char Triangle_Down_Pointing_Black = TRIANGLE_DOWN_POINTING_BLACK;
    public const char Triangle_Left_Pointing_Black = TRIANGLE_LEFT_POINTING_BLACK;
    public const char Triangle_Right_Pointing_Black = TRIANGLE_RIGHT_POINTING_BLACK;
    
    public const char Arrow_Up = ARROW_UP;
    public const char Arrow_Down = ARROW_DOWN;
    public const char Arrow_Left = ARROW_LEFT;
    public const char Arrow_Right = ARROW_RIGHT;
    
    public const char Bullet = BULLET;
    public const char Heart = HEART;
    public const char Diamond = DIAMOND;
    public const char Club = CLUB;
    public const char Spade = SPADES;
    
    // Additional legacy names for mixed single/double T-junctions
    public const char SingleLineTDoubleUp = SINGLE_LINE_T_DOUBLE_UP;
    public const char SingleLineTDoubleDown = SINGLE_LINE_T_DOUBLE_DOWN;
    public const char SingleLineTDoubleLeft = SINGLE_LINE_T_DOUBLE_LEFT;
    public const char SingleLineTDoubleRight = SINGLE_LINE_T_DOUBLE_RIGHT;
    public const char DoubleLineTSingleUp = DOUBLE_LINE_T_SINGLE_UP;
    public const char DoubleLineTSingleDown = DOUBLE_LINE_T_SINGLE_DOWN;
    public const char DoubleLineTSingleLeft = DOUBLE_LINE_T_SINGLE_LEFT;
    public const char DoubleLineTSingleRight = DOUBLE_LINE_T_SINGLE_RIGHT;
    
    // Block character legacy names with underscores
    public const char Block_Solid = BLOCK_SOLID;
    public const char Block_Dense = BLOCK_DENSE;
    public const char Block_Medium = BLOCK_MIDDLE;
    public const char Block_Middle = BLOCK_MIDDLE;
    public const char Block_Sparse = BLOCK_SPARSE;
}