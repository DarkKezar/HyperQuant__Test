namespace Core.Constants;

public static class BitfinexApiConstants
{
    public static class Urls
    {
        public const string BaseUrl = "https://api-pub.bitfinex.com/v2/";
        public const string WebSocketUrl = "wss://api-pub.bitfinex.com/ws/2";

        public static class Endpoints
        {
            public const string Trades = "trades/";
            public const string Candles = "candles/";
        }
    }

    public static class WebSocketData
    {
        public static class Events
        {
            public const string Subscribe = "subscribe";
            public const string Unsubscribe = "unsubscribe";
        }

        public static class Channels
        {
            public const string Trades = "trades";
            public const string Candles = "candles";
        }

        public static class ResponseFields
        {
            public const string Event = "event";
            public const string ChannelId = "chanId";
            public const string Pair = "pair";
            public const string Channel = "channel";
        }
    }

    public static class Symbols
    {
        public const string BtcUsd = "tBTCUSD";
        // e.t.c.
    }
}