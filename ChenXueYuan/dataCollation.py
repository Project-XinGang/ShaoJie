from datetime import datetime
import pandas as pd

# 读取数据
data = pd.read_csv('../data/1000_data.csv')

# 创建DataFrame
df = pd.DataFrame(data)

format_dates = []
for date in df['Date']:
    # 将时间字符串转换为datetime对象
    temp_date = datetime.strptime(date, "%Y/%m/%d-%H:%M:%S")
    # 移除秒数并将datetime对象格式化为标准时间字符串
    format_date = temp_date.strftime("%Y-%m-%d %H:%M")
    format_dates.append(format_date)

df['Date'] = format_dates
# 使用pivot_table函数转换为需求表格格式
pivot_df = df.pivot_table(index='Date', columns='ID', aggfunc=lambda x: x)

# 填充缺失值为''
# pivot_df = pivot_df.fillna('')

path = 'standard_1000_data.csv'
pivot_df.to_csv(path)
