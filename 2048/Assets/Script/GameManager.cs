using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject[] n;
    public GameObject Quit;
    public Text Score, BestScore,ResultScore;

    List<int> fir=new List<int>();
    int x, y,j,i,score,ux,uy;
    GameObject[,] Square = new GameObject[4, 4];
    //GameObject[,] UndoSquare = new GameObject[4, 4];
    //GameObject[,] UndoSquare2 = new GameObject[4, 4];
    GameObject[,] Plus = new GameObject[4, 4];

    float gX,gY;

    Vector3 firstPos,gap;
    bool wait,move,stop;
    void Start()
    {
       // float height=Camera.main.orthographicSize*2;
       // float width=height*Screen.width/Screen.height;
        Spawn(); 
        Spawn();
        BestScore.text=PlayerPrefs.GetInt("BestScore").ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
        //뒤로가기
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        //타일에 움직일 것이 없으면 스톱! 이 밑으로 실행 안됨
        if(stop) return;

        //마우스로 눌렀을때 || 모바일로 터치했을때
        if(Input.GetMouseButtonDown(0) || (Input.touchCount==1 && Input.GetTouch(0).phase==TouchPhase.Began)){

            wait =true;
            //둘 중 하나 값만 들어가서 작동해야함
            //만약 마우스클릭이 0이면 마우스, 아니면 터치 입력  /벡터2(x,y)값이라 벡터3(x,y,z)로 형변환해줌
            firstPos=Input.GetMouseButtonDown(0)? Input.mousePosition : (Vector3)Input.GetTouch(0).position;
        }

        //마우스 클릭한 상태 좌표, 모바일 터치 방향감지
        if(Input.GetMouseButton(0) || (Input.touchCount==1 && Input.GetTouch(0).phase==TouchPhase.Moved)){
            //gap=두번째포지션-첫번째포지션
            gap=(Input.GetMouseButton(0)? Input.mousePosition : (Vector3)Input.GetTouch(0).position) -firstPos;

            //magnitude 백터의 길이 반환, x와 y의 길이값
            if(gap.magnitude<100) return;
            //Normalize 백터의 정규화=방향벡터
            gap.Normalize();

             //무한실행 막기 위함 1번만 실행
            if(wait){ 
                wait=false;
                gY=gap.y; gX=gap.x;
                //위로 이동시 [0,0] [0,1] [0,2] [0,3]
                if(gap.y>0 && gap.x>-0.5f && gap.x<0.5f){
                    for(x=0;x<=3;x++){
                        for(y=0;y<=2;y++){
                           for(i=3;i>=y+1;i--){
                                MoveOrCombine(x,i-1,x,i);
                           }
                        }
                    }
                }
                //아래로 이동시
                else if(gap.y<0 && gap.x>-0.5f && gap.x<0.5f){
                    for(x=0;x<=3;x++){
                        for(y=3;y>=1;y--){
                            for(i=0;i<=y-1;i++){
                                MoveOrCombine(x,i+1,x,i);
                            }
                        }
                    }
                }
                //오른쪽
                else if(gap.x>0 && gap.y>-0.5f && gap.y<0.5f){
                    for(y=0;y<=3;y++){
                        for(x=0;x<=2;x++){
                            for(i=3;i>=x+1;i--){
                                MoveOrCombine(i-1,y,i,y);
                            }
                        }
                    }
                }
                //왼쪽 [0,0], [1,0], [2,0], [3,0]
                else if(gap.x<0 && gap.y>-0.5f && gap.y<0.5f){
                    for(y=0;y<=3;y++){
                        for(x=3;x>=1;x--){
                            for(i=0;i<=x-1;i++){
                                MoveOrCombine(i+1,y,i,y);
                            }    
                        }
                    }
                }else return;
                
                //numBox가 벽에 있으면 
                if(move){
                    move=false;
                    Spawn();
                    int tile=0 ,count =0;
                    //점수 추가, 최고기록
                    if(score>0){
                        Score.text=(int.Parse(Score.text)+score).ToString();
                        if(PlayerPrefs.GetInt("BestScore",0)<int.Parse(Score.text)) PlayerPrefs.SetInt("BestScore",int.Parse(Score.text));
                        BestScore.text=PlayerPrefs.GetInt("BestScore").ToString();
                       // score=0;
                    }

                    // 이동이 끝나면 컴바인해제
                    for(x=0;x<=3;x++){
                        for(y=0;y<=3;y++){
                            if(Square[x,y]==null) {tile++; continue;}
                            else if(Square[x,y].tag=="Combine") Square[x,y].tag="Untagged";
                        }
                    }
                    //타일이 다 찼다고 게임오버 X, 모든 타일이 차면 가로와 세로 인접한 타일 개수 카운트                  
                    if(tile==0){
                        //가로검사
                        for(y=0;y<=3;y++) for(x=0;x<=2;x++) if(Square[x,y].name==Square[x+1,y].name) count++;
                        //세로검사
                        for(x=0;x<=3;x++) for(y=0;y<=2;y++) if(Square[x,y].name==Square[x,y+1].name) count++;
                        //인접 타일이 서로 동일시 움직임 가능하니 검사하고 정말 움직일게 없을때 실행
                        if(count==0){stop=true; ResultScore.text=Score.text; Quit.SetActive(true); return;}
                        
                    }
                    
                }
            }
        }
    }

    //[x1,y1] 이동전 좌표, [x2,y2]이동 될 좌표
    void MoveOrCombine(int x1,int y1,int x2,int y2){
        //처음 실행시 움직임
        //이동 될 좌표가 비어있고 이동 전 좌표가 존재하면 이동
        if(Square[x2,y2]==null && Square[x1,y1]!=null){
            //transform.Tlanslate는 그냥 이동해라~ //Vector3.MoveTowards 특정 좌표까지 이동해라~
                                                                //이동 전 좌표,이동 될 자표,속도
            //Square[x1,y1].transform.position=Vector3.MoveTowards(Square[x1,y1].transform.position,new Vector3(1.2f * x2 - 1.8f, 1.2f * y2 - 1.8f, 0),10000);
            move=true;
           // UndoSquare[x1,y1]=Square[x1,y1];
            //좌표이동
            Square[x1,y1].GetComponent<Moving>().Move(x2,y2,false);
            //이동 좌표값으로 넘겨줌
        //   UndoSquare[x2,y2]=Square[x1,y1];
            Square[x2,y2]=Square[x1,y1];         
            Square[x1,y1]=null;
        }

        //이동 전 후 값이 같은 수일때 결합
        if(Square[x1,y1]!=null && Square[x2,y2]!=null && Square[x1,y1].name==Square[x2,y2].name
                && Square[x1,y1].tag!="Combine"&& Square[x2,y2].tag!="Combine"){
            Debug.Log("결합"+x1+y1+" : "+x2+y2);
            move=true;
            //이동될 좌표의 이름과 도형의 이름이 같으면 => j값 가지고 나가기
            for(j=0;j<=16;j++) if(Square[x2,y2].name==n[j].name+"(Clone)") {
                break;
            }
            //UndoSquare[x1,y1]=Square[x1,y1];
            //DontDestroyOnLoad(UndoSquare[x1,y1]);
            Square[x1,y1].GetComponent<Moving>().Move(x2,y2,true);
            //UndoSquare[x2,y2]=Square[x1,y1];
            //이동될 좌표의 상자 삭제
            Destroy(Square[x2,y2]);
            Square[x1,y1]=null;
            Square[x2,y2]=Instantiate(n[j+1],new Vector3(1.2f * x2 - 1.8f, 1.2f * y2 - 1.8f, 0),Quaternion.identity);
            //GameObject.Find("Clone").transform
            Square[x2,y2].tag="Combine";
            Square[x2,y2].GetComponent<Animator>().SetTrigger("Combine");
            //Plus[x2,y2]=Square[x2,y2];

            //같은 상자 결합 후 점수 추가
            //Mathf.Pow : 거듭제곱 2의 j+2제곱
            score=(int)Mathf.Pow(2,j+2);
        }
    }
    public void Undo(){
        //새로생긴 블럭 삭제
        if(Square[ux,uy]!=null){
            Debug.Log("삭제"+ux+uy);
            Destroy(Square[ux,uy]);
        }
        Debug.Log("Y: "+gY+" X: "+gX);
        if(gY<0 && gX>-0.5f && gX<0.5f){//아래에서 위로
            for(x=0;x<=3;x++) for(y=0;y<=2;y++) for(i=3;i>=y+1;i--) UndoMove(x,i-1,x,i);  
        }else if(gY>0 && gX>-0.5f && gX<0.5f){//위에서 아래로
            for(x=0;x<=3;x++) for(y=3;y>=1;y--) for(i=0;i<=y-1;i++) UndoMove(x,i+1,x,i);
        }else if(gX<0 && gY>-0.5f && gY<0.5f){//오른쪽에서 왼쪽
            for(y=0;y<=3;y++) for(x=0;x<=2;x++) for(i=3;i>=x+1;i--) UndoMove(i-1,y,i,y);
        }else if(gX>0 && gY>-0.5f && gY<0.5f){//왼쪽에서 오른쪽
            for(y=0;y<=3;y++) for(x=3;x>=1;x--) for(i=0;i<=x-1;i++) UndoMove(i+1,y,i,y);
        }else return;

        if(move){
            move=false;
            Spawn();
            int tile=0 ,count =0;
            //점수 추가, 최고기록
            if(score>0){
                Score.text=(int.Parse(Score.text)+score).ToString();
                if(PlayerPrefs.GetInt("BestScore",0)<int.Parse(Score.text)) PlayerPrefs.SetInt("BestScore",int.Parse(Score.text));
                    BestScore.text=PlayerPrefs.GetInt("BestScore").ToString();
                       // score=0;
                }

                // 이동이 끝나면 컴바인해제
                for(x=0;x<=3;x++){
                    for(y=0;y<=3;y++){                       
                        if(Square[x,y]==null) {tile++; continue;}
                        else if(Square[x,y].tag=="Combine") Square[x,y].tag="Untagged";
                    }
                }
                //타일이 다 찼다고 게임오버 X, 모든 타일이 차면 가로와 세로 인접한 타일 개수 카운트                  
                if(tile==0){
                    //가로검사
                    for(y=0;y<=3;y++) for(x=0;x<=2;x++) if(Square[x,y].name==Square[x+1,y].name) count++;
                    //세로검사
                    for(x=0;x<=3;x++) for(y=0;y<=2;y++) if(Square[x,y].name==Square[x,y+1].name) count++;
                    //인접 타일이 서로 동일시 움직임 가능하니 검사하고 정말 움직일게 없을때 실행
                    if(count==0){stop=true; ResultScore.text=Score.text; Quit.SetActive(true); return;
                }            
            }    
        }
    }
    void UndoMove(int x1,int y1,int x2,int y2){
        //move=true;
        if(Plus[x1,y1]!=null){
            for(j=0;j<=16;j++) if(Square[x1,y1].name==n[j].name+"(Clone)") {
                break;
            }
            Debug.Log("결합존재"+x1+y1 +" : "+x2+y2);
            //결합된 상자 삭제
            Destroy(Square[x1,y1]);
            Square[x1,y1]=null;
           // UndoSquare[x1,y1]=Instantiate(n[j-1],new Vector3(1.2f * x1 - 1.8f, 1.2f * y1 - 1.8f, 0),Quaternion.identity);
          //  UndoSquare[x1,y1]=Instantiate(n[j-1],new Vector3(1.2f * x2 - 1.8f, 1.2f * y2 - 1.8f, 0),Quaternion.identity);
        }
        // if(Square[x1,y1]==null){
        //     UndoSquare[x1,y1]=Instantiate(n[j-1],new Vector3(1.2f * x1 - 1.8f, 1.2f * y1 - 1.8f, 0),Quaternion.identity);
        //     UndoSquare[x1,y1].GetComponent<Moving>().Move(x2,y2,false);
        // }
        // if(UndoSquare2[x1,y1]!=null&& UndoSquare2[x2,y2]!=null){
        //     Debug.Log("이동2 "+x1+y1 +" : "+x2+y2);
        //     UndoSquare2[x1,y1].GetComponent<Moving>().Move(x2,y2,false);
        //     Square[x2,y2]=Square[x1,y1];
        //     Square[x1,y1]=null;
        // }
        // //이전위치로 이동
        // if(UndoSquare[x1,y1]!=null&& UndoSquare[x2,y2]!=null){
        //     Debug.Log("이동 "+x1+y1 +" : "+x2+y2);
        //     UndoSquare[x1,y1].GetComponent<Moving>().Move(x2,y2,false);
        //     Square[x2,y2]=Square[x1,y1];         
        //     Square[x1,y1]=null;
        // }
    }
    void Spawn()
    {
        while (true)
        {
            x = Random.Range(0, 4); y = Random.Range(0, 4);
            if (Square[x, y] == null) break;
        }
        // Instantiate: 복사 
        // 0~7중에 0보다 크면n[0]=2 , 0보다 작으면 n[1]=4
        //vector3: 좌표 
        // Quaternion.identity Quaternion 타입의 한 속성으로 api 문서에는 '회전 없음' 또는 부모의 축으로 정렬
        //3d모델에선 원래 회전각도로 적용
        //쿼터니언은 복잡한 수를 기반으로 하고, 직관적으로 이해하기 어려움, 접근하지 않거나 개별 컴포넌트(x,y,z,w) 수정하지 않음
        Square[x, y] = Instantiate(Random.Range(0, 8)>0? n[0]: n[1], new Vector3(1.2f * x - 1.8f, 1.2f * y - 1.8f, 0),Quaternion.identity);
        //Square[x, y] = Instantiate(Random.Range(0, int.Parse(Score.text)>800? 4: 8) > 0 ? n[0] : n[1], new Vector3(1.2f * x - 1.8f, 1.2f * y - 1.8f, 0),
        // Quaternion.identity, GameObject.Find("Canvas").transform); 캔버스 안에다 생성
        Square[x,y].GetComponent<Animator>().SetTrigger("Spawn");
       // ux=x; uy=y;
    }
    public void Restart()
    {
        //재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
