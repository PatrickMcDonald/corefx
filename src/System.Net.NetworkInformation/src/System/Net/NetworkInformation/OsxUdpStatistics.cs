﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.Net.NetworkInformation
{
    internal class OsxUdpStatistics : UdpStatistics
    {
        private readonly long _datagramsReceived;
        private readonly long _datagramsSent;
        private readonly long _incomingDiscarded;
        private readonly long _incomingErrors;
        private readonly int _numListeners;

        public OsxUdpStatistics()
        {
            Interop.Sys.UdpGlobalStatistics statistics;
            if (Interop.Sys.GetUdpGlobalStatistics(out statistics) == -1)
            {
                throw new NetworkInformationException((int)Interop.Sys.GetLastError());
            }

            _datagramsReceived = (long)statistics.DatagramsReceived;
            _datagramsSent = (long)statistics.DatagramsSent;
            _incomingDiscarded = (long)statistics.IncomingDiscarded;
            _incomingErrors = (long)statistics.IncomingErrors;
            Debug.Assert(statistics.UdpListeners >= 0);
            _numListeners = (int)Math.Min(int.MaxValue, statistics.UdpListeners);
        }

        public override long DatagramsReceived { get { return _datagramsReceived; } }

        public override long DatagramsSent { get { return _datagramsSent; } }

        public override long IncomingDatagramsDiscarded { get { return _incomingDiscarded; } }

        public override long IncomingDatagramsWithErrors { get { return _incomingErrors; } }

        public override int UdpListeners { get { return _numListeners; } }

    }
}