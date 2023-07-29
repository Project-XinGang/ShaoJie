from asyncua import Client, ua
import asyncio


async def get_text():
    async with Client(url='opc.tcp://10.160.8.221:49320', timeout=3600000) as client:
        while True:
            node1 = client.get_node('ns=2;s=通道 2.设备 1.O2_OUT')
            # node2 = client.get_node('ns=2;s=通道 2.设备 1.O2_IN')
            # node3 = client.get_node('ns=2;s=SJJ8.PLC_YK.LI302')
            data_value = await node1.read_data_value()
            # print(data_value)
            O2_OUT = data_value.Value.Value
            timeStamp = data_value.SourceTimestamp
            # O2_IN = await node2.read_value()
            # li302=await node3.read_value()
            print("O2_OUT : ", O2_OUT, "\t TimeStamp : ", timeStamp)
            await asyncio.sleep(1)
            # await node.write_value(ua.DataValue(ua.Variant(value + 1, ua.VariantType.Int16, None, None), None))
            # await asyncio.sleep(1)

            # 保存到csv文件



def main():
    loop = asyncio.get_event_loop()
    task = get_text()
    loop.run_until_complete(task)
    loop.close()


if __name__ == '__main__':
    main()

