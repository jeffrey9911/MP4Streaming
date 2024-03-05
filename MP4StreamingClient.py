import asyncio
import json
from aiohttp import ClientSession
from aiortc import RTCPeerConnection, RTCSessionDescription, MediaStreamTrack

async def create_offer(pc):
    offer = await pc.createOffer()
    await pc.setLocalDescription(offer)
    return offer

async def main():
    pc = RTCPeerConnection()

    # 处理从服务器接收到的媒体流
    @pc.on("track")
    def on_track(track):
        print(f"Received {track.kind} track")
        if track.kind == "video":
            # 这里处理接收到的视频轨道
            pass

    # 创建SDP offer
    offer = await create_offer(pc)

    # 使用aiohttp发送offer到服务器
    async with ClientSession() as session:
        async with session.post("http://localhost:8080/offer", json={"sdp": offer.sdp, "type": offer.type}) as resp:
            answer = await resp.json()
            print("Received answer:")
            print(answer)

            # 设置远程描述
            await pc.setRemoteDescription(RTCSessionDescription(sdp=answer["sdp"], type=answer["type"]))

    # 等待一段时间，以便流媒体可以开始
    await asyncio.sleep(30)

    # 关闭连接
    await pc.close()

if __name__ == "__main__":
    asyncio.run(main())
