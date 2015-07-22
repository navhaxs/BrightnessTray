/*
	This file is part of BrightnessTray.
	Copyright (C) 2015 Jeremy Wong

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License along
	with this program; if not, write to the Free Software Foundation, Inc.,
	51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
using System;

namespace BrightnessTray
{
    // methods to keep the monitor on
    static class Caffeine
    {
        private static uint fPreviousExecutionState;

        public static void lockSleepMode()
        {
            // Set new state to prevent monitor off, and thus keep the system from sleeping
            fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_DISPLAY_REQUIRED);
            if (fPreviousExecutionState == 0)
            {
                #if DEBUG
                throw new Exception("SetThreadExecutionState failed.");
                #endif
            }
        }

        public static void unlockSleepMode()
        {
            // Return to normal
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
        }
    }
}
