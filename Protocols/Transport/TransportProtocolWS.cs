using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Server.Sockets.Client;

namespace Server.Protocols.Transport {
    [ATransportProtocol("websocket", 8, 13)]
    sealed class TransportProtocolWS : ITransportProtocol {
        static private ITransportProtocol instance = new TransportProtocolWS();

        private TransportProtocolWS() {
        }

        public IPacket Read(SocketClient socket) {
            try {
                bool fin = (socket.Buffer[0] & 0x80) == 0x80;
    
                bool rsv1 = (socket.Buffer[0] & 0x40) == 0x40;
                bool rsv2 = (socket.Buffer[0] & 0x20) == 0x20;
                bool rsv3 = (socket.Buffer[0] & 0x10) == 0x10;
    
                OpCode opCode = (OpCode)((socket.Buffer[0] & 0x8) | (socket.Buffer[0] & 0x4) | (socket.Buffer[0] & 0x2) | (socket.Buffer[0] & 0x1));

                bool mask = (socket.Buffer[1] & 0x80) == 0x80;
    
                byte payload = (byte)((socket.Buffer[1] & 0x40) | (socket.Buffer[1] & 0x20) | (socket.Buffer[1] & 0x10) | (socket.Buffer[1] & 0x8) | (socket.Buffer[1] & 0x4) | (socket.Buffer[1] & 0x2) | (socket.Buffer[1] & 0x1));
                ulong length = 0;
    
                switch(payload) {
                    case 126:
                        byte[] bytesUShort = socket.Receive(2);
                        if(bytesUShort != null) {
                            length = BitConverter.ToUInt16(bytesUShort.Reverse().ToArray(), 0);
                        }
                        break;
                    case 127:
                        byte[] bytesULong = socket.Receive(8);
                        if(bytesULong != null) {
                            length = BitConverter.ToUInt16(bytesULong.Reverse().ToArray(), 0);
                        }
                        break;
                    default:
                        length = payload;
                        break;
                }
    
                byte[] maskKeys = null;
                if(mask) {
                    maskKeys = socket.Receive(4);
                }

                WSPacket packet = null;

                if(length > 0) {
                    byte[] data = socket.Receive((int)length);
        
                    if(mask) {
                        for(int i = 0; i < data.Length; ++i) {
                            data[i] = (byte)(data[i] ^ maskKeys[i % 4]);
                        }
                    }

                    ushort closeCode = 0;
                    if(opCode == OpCode.Close && data.Length == 2) {
                        closeCode = BitConverter.ToUInt16(((byte[])data.Clone()).Reverse().ToArray(), 0);
                    }

                    if(closeCode != 0) {
                        packet = new WSPacket(data, opCode, closeCode);
                    } else {
                        packet = new WSPacket(data, opCode);
                    }
                } else {
                    packet = new WSPacket(null, opCode);
                }

                return packet;

            } catch(Exception ex) {
                Log.Add("websocket transport protocol Read exception: " + ex.ToString());
            }

            return null;
        }

        public byte[] Send(byte[] binary) {
            try {
                ulong headerLength = 2;
                byte[] data = binary;
    
                bool mask = false;
                byte[] maskKeys = null;
    
                if(mask) {
                    headerLength += 4;
                    data = (byte[])data.Clone();
    
                    maskKeys = new byte[4];
                    for(int i = 0; i < 4; ++i) {
                        maskKeys[i] = (byte)Rand.Next(byte.MinValue, byte.MaxValue);
                    }
    
                    for(int i = 0; i < data.Length; ++i) {
                        data[i] = (byte)(data[i] ^ maskKeys[i % 4]);
                    }
                }
    
                byte payload;
                if(data.Length >= 65536) {
                    headerLength += 8;
                    payload = 127;
                } else if(data.Length >= 126) {
                        headerLength += 2;
                        payload = 126;
                    } else {
                        payload = (byte)data.Length;
                    }
    
                byte[] header = new byte[headerLength];
    
                header[0] = 0x80 | 0x1;
                if(mask) {
                    header[1] = 0x80;
                }
                header[1] = (byte)(header[1] | payload & 0x40 | payload & 0x20 | payload & 0x10 | payload & 0x8 | payload & 0x4 | payload & 0x2 | payload & 0x1);
    
                if(payload == 126) {
                    byte[] lengthBytes = BitConverter.GetBytes((ushort)data.Length).Reverse().ToArray();
                    header[2] = lengthBytes[0];
                    header[3] = lengthBytes[1];

                    if(mask) {
                        for(int i = 0; i < 4; ++i) {
                            header[i + 4] = maskKeys[i];
                        }
                    }
                } else if(payload == 127) {
                        byte[] lengthBytes = BitConverter.GetBytes((ulong)data.Length).Reverse().ToArray();
                        for(int i = 0; i < 8; ++i) {
                            header[i + 2] = lengthBytes[i];
                        }
                        if(mask) {
                            for(int i = 0; i < 4; ++i) {
                                header[i + 10] = maskKeys[i];
                            }
                        }
                    }
    
                return header.Concat(data).ToArray();

            } catch(Exception ex) {
                Log.Add("websocket transport protocol Send exception: " + ex.ToString());
            }

            return null;
        }

        public byte[] Ping() {
            try {
                byte[] header = new byte[2];

                header[0] = 0x80 | 0x9;
                header[1] = 0;
    
                return header;
            } catch(Exception ex) {
                Log.Add("websocket transport protocol Ping exception: " + ex.ToString());
            }

            return null;
        }
    }

    sealed class WSPacket : IPacket {
        private byte[]    data;
        private OpCode    opCode;
        private CloseCode closeCode;

        public WSPacket(byte[] data, OpCode opCode) {
            this.data = data;
            this.opCode = opCode;
            this.closeCode = CloseCode.Undefined;
        }

        public WSPacket(byte[] data, OpCode opCode, ushort closeCode) {
            this.data = data;
            this.opCode = opCode;
            if(closeCode >= 1000 && closeCode <= 1010) {
                this.closeCode = (CloseCode)closeCode;
            } else {
                this.closeCode = CloseCode.WrongCode;
            }
        }

        public byte[] Data {
            get { return this.data; }
        }

        public OpCode OpCode {
            get { return this.opCode; }
        }

        public CloseCode CloseCode {
            get { return this.closeCode; }
        }
    }

    public enum OpCode {
        Continuation = 0x0,
        Text         = 0x1,
        Binary       = 0x2,
        Reserved3    = 0x3,
        Reserved4    = 0x4,
        Reserved5    = 0x5,
        Reserved6    = 0x6,
        Reserved7    = 0x7,
        Close        = 0x8,
        Ping         = 0x9,
        Pong         = 0xA,
        ReservedB    = 0xB,
        ReservedC    = 0xC,
        ReservedD    = 0xD,
        ReservedE    = 0xE,
        ReservedF    = 0xF
    }

    public enum CloseCode {
        Undefined        = 0,
        WrongCode        = 1,
        Normal           = 1000,
        Away             = 1001,
        ProtocolError    = 1002,
        WrongData        = 1003,
        Reserved4        = 1004,
        Reserved5        = 1005,
        Reserved6        = 1006,
        WrongConsistency = 1007,
        PolicyViolated   = 1008,
        MessageTooBig    = 1009,
        WrongExtensions  = 1010
    }
}