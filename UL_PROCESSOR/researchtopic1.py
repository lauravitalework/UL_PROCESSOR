# -*- coding: utf-8 -*-
"""
Created on Wed May  9 23:20:14 2018

@author: goldw
"""

import pandas as pd
import numpy as np
from scipy.io import wavfile
from numpy.fft import fft, ifft, fftshift
import os

directory = "D:\\3_6"
aligned = "D:\\3_6_aligned\\"
Date = "03/03/2017"
LENA = "D:\\LENAACTIVITYBLOCKALL.csv"
master_file = "e20170306_110114_014867.wav"
ok = pd.read_csv(LENA)
ok = ok.drop_duplicates("File_Name")
clock_time = ok["Clock_Time_TZAdj"]

def ConvertSectoDay(n):
	n = n % (24 * 3600)
	hour = n // 3600

	n %= 3600
	minutes = n // 60

	n %= 60
	seconds = n
	
	return (str(int(hour)) + ":" + str(int(minutes)) + ":" + str(int(seconds)))

def time_align(y, y2, LENA, filepath):
    """
    y = the signal path in system of the longest relative time.
    y2 = the signal path in system for which you would like to align to y.
    LENA = the .csv file that contains the clock time so the correlation function does not go on forever.
    filepath = the path and name of the new aligned signal.
    """
    #make sure x is the longest file
    def cross_correlation_using_fft(x, y):
        f1 = fft(x)
        f2 = fft(np.flipud(y))
        cc = np.real(ifft(f1 * f2))
        return fftshift(cc)
     
    #make sure x is the longest file
    def compute_shift(x, y):
        assert len(x) == len(y)
        c = cross_correlation_using_fft(x, y)
        assert len(c) == len(x)
        zero_index = int(len(x) / 2) - 1
        shift = zero_index - np.argmax(c)
        return shift
    
    fs, y_sig = wavfile.read(y) #long signal
    fs, y2_sig = wavfile.read(y2)
    
    metadata = pd.read_csv(LENA)
    
    metadata = metadata.drop_duplicates("File_Name")
    clock_time = ok["Clock_Time_TZAdj"]
    File_Name = metadata["File_Name"]
    
    for x in range(0,len(File_Name)):
        place = File_Name.iloc[x]
        if y[7:30] == place[0:23]:
            print("yes")
            date_time1 = clock_time.iloc[x]
            time1 = date_time1[-8:]
            hour1 = (int(time1[:2])*60*60)
            minute1 = (int(time1[3:5])*60)
            second1 = int(time1[6:8])
            total_seconds1 = hour1 + minute1 + second1
            print(total_seconds1)
            
    for a in range(0, len(File_Name)):
        place = File_Name.iloc[a]
        if y2[7:30] == place[0:23]:
            print("yes2")
            date_time2 = clock_time.iloc[a]
            time2 = date_time2[-8:]
            hour2 = (int(time2[:2])*60*60)
            minute2 = (int(time2[3:5])*60)
            second2 = int(time2[6:8])
            total_seconds2 = hour2 + minute2 + second2
            print(total_seconds2)
            
    diff = abs(total_seconds1 - total_seconds2)
    
    
    if diff == 0:
        del y_sig, y2_sig
        return 0, time1, time1
    else:
        total_samples = diff*fs;
        
        if len(y_sig) > len(y2_sig):
            y2_pad = np.pad(y2_sig, (total_samples,0), 'constant')
        else:
            y2_pad = np.pad(y2_sig, (total_samples,0), 'constant')
        
        y_window = y_sig[abs(total_samples-(fs*30*60)):(total_samples+(fs*30*60))]
        y2_window = y2_pad[abs(total_samples-(fs*30*60)):(total_samples+(fs*30*60))]
        
        lag = compute_shift(y2_window,y_window)
        print(lag)
        total_samples += lag
        
        y2_sig = np.pad(y2_sig, (total_samples,0), 'constant')
        
        wavfile.write(filepath,fs,y2_sig)
        
        total_seconds2 += (lag/fs)
        
        del y_sig, y2_sig, y_window, y2_window
        
        return (lag/fs), time2, ConvertSectoDay(total_seconds2)

Headers = ["1", "2", "3", "4", "5", "6", "7"]
df = pd.DataFrame()
master = os.path.join(directory, master_file)

for file in os.listdir(directory):
    if file.endswith(".wav"):
        print(file)
        sync = os.path.join(directory, file)
        series = list()
        series2 = list()
        if file != master_file:
            filepath = aligned + file[0:23] + "_aligned.wav"
            seconds, lena_time, actual_time = time_align(master, sync, LENA, filepath)
            series.append(file)
            series.append(Date)
            series.append(lena_time)
            series.append(actual_time)
            series.append(seconds)
            series.append(0)
            series.append("FALSE")
            series2.append(series)
            df = df.append(series2)
        elif file == master_file:
            filepath = aligned + file[0:23] + "_aligned.wav"
            seconds, lena_time, actual_time = time_align(master, master, LENA, filepath)
            series.append(file)
            series.append(Date)
            series.append(lena_time)
            series.append(lena_time)
            series.append(0)
            series.append(0)
            series.append("TRUE")
            series2.append(series)
            df = df.append(series2)

df.to_csv((directory + "\\may26_2017.csv"))