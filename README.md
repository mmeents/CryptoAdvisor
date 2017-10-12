# CryptoAdvisor
Alt-Coin Crypto Trader on Poloniex Application  https://github.com/mmeents/CryptoAdvisor/blob/master/Install/MakeInstall/MakeInstall-SetupFiles/MakeInstall.msi is the install.

Utilizes https://github.com/kripod/PoloniexApi.Net for communication with Poloniex Exchange.

Set of 3 Queues that timers are set up to pop off an item and process it.  

Main Queues are:

PoloQueue -- Rate limited to not exceed 6 requests per sec.
CommandQueue
Live Feed Queue

Polo API Keys are stored in the users AppData folder and are password defined AES generated encrypted.

App maintains a Dictionary of appMarket objects for each market returned by Marketsummary call.

All commands running from PoloQueue save to LastObject within appMarket above for processing during a CommandQueue tick.   

Live when it's working also save's the ticks to the Dictionary 

Main Timers

Draw Timer, draws everything on every tick.
Polo and Command timers for execution of items off the queue.
Feed Timers that process the details coming of the live feed.
TimeTimer for algo trading and variable recalculating. 

Needs work but it's a start.  

Matt

Tips to

BTC: 19rgTZZ5KvwFMbL77XEXE3k4S26MZMt6um

ETH: 0x2b8C4592b7205587CF0472Ac36C16DAd2785b07d

LTC: Lhy2TkUirBWWMKnD6htRxHEpTwEiMizcft
