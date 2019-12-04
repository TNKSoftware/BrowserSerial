void setup()
{
    Serial.begin(115200);
    Serial.println("Hello!");
}

char str[256]; int spos = 0;
int count = 0;

void loop()
{
    while(Serial.available()){
        char c = Serial.read();
        str[spos] = c;
        if(c == '\n'){
            str[spos] = '\0';
            if(++count >= 3){
                Serial.println(str);
                count = 0;
            }
            spos = 0;
        }else if(spos >= 255){
            spos = 0;
        }else{
            spos++;
        }
    }
}