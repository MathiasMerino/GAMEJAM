"""Música original sintetizada para Rabbitscape. No utiliza muestras de la intro.
Requiere numpy/scipy. Bucles estéreo de 48 s, PCM 16 bits a 22050 Hz.
"""
from pathlib import Path
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, sosfilt
SR=22050; DUR=48; N=SR*DUR
rng=np.random.default_rng(734)
t=np.arange(N)/SR
out=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio'
out.mkdir(parents=True,exist_ok=True)
def tone(f):
    return np.sin(2*np.pi*round(f*DUR)/DUR*t)
def add_event(y,when,seconds,f,amp,pan=0,metal=False):
    z=np.arange(int(seconds*SR))/SR
    env=(1-np.exp(-z*35))*np.exp(-z/(seconds/5))
    a=np.sin(2*np.pi*f*z)
    if metal:a+=.25*np.sin(2*np.pi*f*2.71*z)+.12*np.sin(2*np.pi*f*4.13*z)
    a*=env*amp
    idx=(int(when*SR)+np.arange(len(z)))%N
    for ch,gain in enumerate((np.sqrt((1-pan)/2),np.sqrt((1+pan)/2))):y[idx,ch]+=a*gain
for name in ['Laboratorio','Acecho','FalsaCalma']:
    y=np.zeros((N,2));calma=name=='FalsaCalma';tension=name=='Acecho'
    # Quintas abiertas y una segunda tenue; resonancias de maquinaria, sin voces.
    drone=.1*tone(73.416)+.045*tone(110)+.025*tone(146.832)
    drone+=(.009 if calma else .018)*tone(77.782)
    drone*=.8+.2*np.sin(2*np.pi*t/12)
    noise=sosfilt(butter(2,[180,1300],fs=SR,btype='bandpass',output='sos'),rng.normal(size=N))
    for ch in range(2):y[:,ch]=np.roll(drone,ch*157)+.012*np.roll(noise,ch*401)
    notes=[293.665,220,311.127,349.228,220,146.832]
    if calma:notes=[293.665,440,349.228,329.628,440,293.665]
    for i,note in enumerate(notes*2):add_event(y,i*4+1,5,note,.06 if calma else .04,(-1)**i*.45,True)
    beat=.5 if tension else 2
    for i,when in enumerate(np.arange(0,DUR,beat)):
        if calma:continue
        add_event(y,when,.5,48 if i%2==0 else 63,.12 if tension else .045)
        if tension and i%2: add_event(y,when+.25,.16,580,.02,(-1)**i*.3,True)
    # Ecos circulares y bordes suavizados: no hay chasquidos al repetir.
    y+=np.roll(y,int(.375*SR),axis=0)*.18+np.roll(y,int(.75*SR),axis=0)*.08
    y=np.tanh(y*1.6)
    fade=int(.012*SR);y[:fade]*=np.linspace(0,1,fade)[:,None];y[-fade:]*=np.linspace(1,0,fade)[:,None]
    wavfile.write(out/(name+'.wav'),SR,(np.clip(y,-1,1)*32767).astype(np.int16))
    print(name,'peak dBFS',round(20*np.log10(abs(y).max()),1),'RMS dBFS',round(20*np.log10(np.sqrt(np.mean(y*y))),1))
