using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving : MonoBehaviour
{
    bool move,_combine;
    int _x2,_y2;
    void Update()
    {
        if(move) Move(_x2,_y2,_combine);
    }
    public void Move(int x2,int y2,bool combine){
        //한 프레임씩 이동(무한반복)
        move=true; _x2=x2; _y2=y2;
        _combine = combine;
        transform.position=Vector3.MoveTowards(transform.position,new Vector3(1.2f * x2 - 1.8f, 1.2f * y2 - 1.8f, 0),0.3f);
        //다음 지점에 도착시 반복정지
        if(transform.position==new Vector3(1.2f*x2 -1.8f,1.2f *y2-1.8f,0)){
            move=false;
            if(combine){
                _combine=false;
                //겹쳐지면 게임오프젝트 파괴 
                Destroy(gameObject);
            }
        } 
    }


}
