using Cairo;
using Gdk;
using Gtk;
using static Gdk.EventMask;
using Color = Cairo.Color;
using static System.Console;
using System.Collections.Generic;
using System;
using Key = Gdk.Key;
using static Direction;
// git remote set-url origin https://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@github.com/HMkrtich/2048.git
enum Direction{
    Right,
    Left,
    Up,
    Down,
    Stay
};
class Tile{
    int value=0;
    public Tile(){}
    
    public void setVal(int value_){
        value=value_;
    }
    public int getVal(){
        return value;
    }
    
}
class MovingBoard{
    Tile[,] board;
    Random rand=new Random();
    int dim;
    int EMPTY=0;
    bool gameOver=false;
    Tile[,] prevBoard;
    int score=0;
    (int ver,int hor)[] dirs={(1,0),    // down
                              (0,1),    // right
                              (-1,0),   // up
                              (0,-1)};  // left
    public MovingBoard(int n){
        board=new Tile[n,n];
        prevBoard=new Tile[n,n];
        for(int i=0;i<n;i++){
            for(int j=0;j<n;j++){
                board[i,j]=new Tile();
                prevBoard[i,j]=new Tile();
            }
        }
        dim=n;
        initBoard();
    }
    void initBoard(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                board[i,j].setVal(EMPTY);
                prevBoard[i,j].setVal(EMPTY);
            }
        }
        int x=rand.Next(0,dim);
        int y=rand.Next(0,dim);
        board[x,y].setVal(2);
    }
    public bool getGameState(){
        return gameOver;
    }
    public int getTile(int i,int j){
        return board[i,j].getVal();
    }
    public int getScore(){
        return score;
    }
    public void repeat(){
        if(permited())
            goBack();
    }
    public void restart(){
        initBoard();
        score=0;
        gameOver=false;
    }
    bool permited(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j].getVal()!=prevBoard[i,j].getVal()){
                    return true;
                }
            }
        }
        return false;
    }
    (int x,int y,int z,int dz) mapping(int i,int j,Direction dir){
        if(dir==Up)return(j,i,j,1);             // up
        if(dir==Down)return(dim-j-1,dim-i-1,dim-j-1,-1);   // down
        if(dir==Left)return(i,j,j,1);           // left
        if(dir==Right)return(dim-i-1,dim-j-1,dim-j-1,-1);  // right

        return(0,0,0,0);
    }
    bool moveDir(Direction dir){
        if(dir==Stay){
            return false;
        }
        bool moved=false;
        for(int i=0;i<dim;i++){ //(0,0),(1,0),(2,0),(3,0)
            for(int j=0;j<dim;j++){
                (int x,int y,int z,int dz)=mapping(i,j,dir);
                // Case 1: the value is empty
                if(board[x,y].getVal()==EMPTY){
                    int k=y;
                    int val=EMPTY;
                    if(dir==Up || dir ==Down){
                        while(true){
                            
                            val=board[z,y].getVal();
                            if(val!=EMPTY){
                                moved=true;
                                updatePrevStep();
                                board[z,y].setVal(EMPTY);
                                break;
                            }
                            z+=dz;
                            if(z>dim-1 || z<0){
                                break;
                            }
                        }
                    }
                    if(dir==Right || dir ==Left){
                        while(true){
                            
                            val=board[x,z].getVal();
                            if(val!=EMPTY){
                                moved=true;
                                updatePrevStep();
                                board[x,z].setVal(EMPTY);
                                break;
                            }
                            z+=dz;
                            if(z>dim-1 || z<0){
                                break;
                            }
                        }
                    }
                    board[x,y].setVal(val);

                }
                // Case 2: the value is not empty
                
                int value=board[x,y].getVal();
                if(value!=EMPTY){
                    int val=EMPTY;
                    if(dir==Up || dir ==Down){
                        while(true){
                            z+=dz;
                            if(z>dim-1 || z<0){
                                break;
                            }
                            val=board[z,y].getVal();
                            if(val==value){
                                moved=true;
                                board[x,y].setVal(2*val);
                                board[z,y].setVal(EMPTY);
                                score+=2*val;
                                break;
                            }
                            else if(val!=EMPTY) break;

                        }
                    }
                    if(dir==Right || dir ==Left){
                        while(true){
                            z+=dz;
                            if(z>dim-1 || z<0){
                                break;
                            }
                            val=board[x,z].getVal();
                            if(val==value){
                                moved=true;
                                board[x,y].setVal(2*val);
                                board[x,z].setVal(EMPTY);
                                score+=2*val;
                                break;
                            }
                            if(val!=EMPTY) break;
                        }
                    }
                }
            }
        }
        
        return moved;
    }
    (int,int) newPos(){
        int nullCount=0;
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j].getVal()==EMPTY){
                    nullCount++;
                }
            }
        }
        WriteLine(nullCount);
        int pos=rand.Next(nullCount);
        
        WriteLine(pos);
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j].getVal()==EMPTY){
                    pos--;
                    if(pos==-1){
                        return (i,j);
                    }
                }
            }
        }
        return (0,0);
    }
    void updatePrevStep(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                prevBoard[i,j].setVal(board[i,j].getVal());
            }
        }
    }
    void goBack(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                board[i,j].setVal(prevBoard[i,j].getVal());
            }
        }
    }
    bool hasEqualNeibour(int i,int j){
        return  (i>0 && board[i-1,j].getVal()==board[i,j].getVal()) 
        ||      (i<dim-1 && board[i+1,j].getVal()==board[i,j].getVal()) 
        ||      (j>0 && board[i,j-1].getVal()==board[i,j].getVal()) 
        ||      (j<dim-1 && board[i,j+1].getVal()==board[i,j].getVal());
    }
    // checking whether the game is over or not
    void over(){
        // if there is a empty tile or 
        // there are two neighbor tiles that
        // have the same value
        // then game is not over
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j].getVal()==0 || hasEqualNeibour(i,j)){
                    return;
                }
            }
        }
        gameOver=true;
    }
    public void play(Direction diri){        
        bool moved=moveDir(diri);
        if(moved){
            int num=rand.Next(1,3);
            int binPow=(int)Math.Pow(2,num);
            (int x,int y)=newPos();
            board[x,y].setVal(binPow);
        }
        // Checking whether the game is Over or not
        over();  
    }
}


class View : DrawingArea {
    // The maximum number that you can get in "3 x 3" grid is 2^(3*3+1)
    //                                        "4 x 4" grid is 2^(4*4+1)
    //                                        "5 x 5" grid is 2^(5*5+1)
    //Colors of every kind of Tiles
    Dictionary<int,Color> colors=new Dictionary<int,Color>(){{0,new Color(230.0/255,230.0/255,250.0/255)},
        {2,new Color(255.0/255,215.0/255,0)},{4,new Color(255.0/255,165.0/255,0)},
        {8,new Color(255.0/255,140.0/255,0)},{16,new Color(255.0/255,69.0/255,0)},
        {32,new Color(255.0/255,160.0/255,122.0/255)},{64,new Color(250.0/255,128.0/255,114.0/255)},
        {128,new Color(233.0/255,150.0/255,122.0/255)},{256,new Color(240.0/255,128.0/255,128.0/255)},
        {512,new Color(205.0/255,92.0/255,92.0/255)},{1024,new Color(255.0/255,127.0/255,80.0/255)},
        {2048,new Color(255.0/255,99.0/255,71.0/255)},{4096,new Color(255.0/255,0,0)},
        {8192,new Color(220.0/255,20.0/255,60.0/255)},{16384,new Color(178.0/255,34.0/255,34.0/255)},
        {32768,new Color(165.0/255,42.0/255,42.0/255)},{65536,new Color(139/255,0,0)},
        {131072,new Color(128.0/255,0,0)},{262144,new Color(0,255.0/255,255.0/255)},
        {524288,new Color(0,255.0/255,255.0/255)},{1048576,new Color(0,139.0/255,139.0/255)},
        {2097152,new Color(0,128.0/255,128.0/255)},{4194304,new Color(47.0/255,79.0/255,79.0/255)},
        {8388608,new Color(25.0/255,25.0/255,112.0/255)},{16777216,new Color(139.0/255,0,139.0/255)},
        {33554432,new Color(112.0/255,128.0/255,144.0/255)},{67108864,new Color(0,0,0)}
    };
    // Specification of the fonts and margins of the "n x n" game 
    Dictionary<int,(int l,int d,int font)> sizes=new Dictionary<int,(int l,int d,int font)>()
                                            {{3,(145,135,35)},{4,(110,100,30)},{5,(87,77,20)}};
    Color black = new Color(0, 0, 0),
          blue = new Color(0, 0, 1),
          light_green = new Color(0.56, 0.93, 0.56),
          grey=new Color(230.0/256,230.0/256,250.0/256),
          pink=new Color(216.0/256,191.0/256,216.0/256),
          white=new Color(1,1,1);

    MovingBoard gm2048;
    int n; // dimension
    bool menu_=true;
    public View(){
        AddEvents((int)EventMask.KeyPressMask);
        AddEvents((int)EventMask.ButtonPressMask);
    }
    protected override  bool OnDrawn (Context c) {
        if(menu_){
            c.SetSourceColor(pink);
            c.Rectangle(x: 30, y: 30, width: 390, height: 540);
            c.Fill();
            c.SetSourceColor(new Color(230.0/255,230.0/255,250.0/255));

            c.Rectangle(x: 130, y: 180, width: 190, height: 60);
            c.Rectangle(x: 130, y: 270, width: 190, height: 60);
            c.Rectangle(x: 130, y: 360, width: 190, height: 60);
            c.Fill();
            // Displaying Restart button
            c.SetSourceColor(blue);
            (int ax1,int  ay1) = (225,210);
            string line = "  Tiny     (3 X 3)";
            c.SetFontSize(25);
            TextExtents tex = c.TextExtents(line);
            c.MoveTo(ax1 - (tex.Width / 2 + tex.XBearing), ay1 - (tex.Height / 2 + tex.YBearing));
            c.ShowText(line);
            (ax1,ay1) = (225,300);
            line = "Classic (4 X 4)";
            c.SetFontSize(25);
            tex = c.TextExtents(line);
            c.MoveTo(ax1 - (tex.Width / 2 + tex.XBearing), ay1 - (tex.Height / 2 + tex.YBearing));
            c.ShowText(line);
            (ax1,ay1) = (225,390);
            line = "     Big    (5 X 5)";
            c.SetFontSize(25);
            tex = c.TextExtents(line);
            c.MoveTo(ax1 - (tex.Width / 2 + tex.XBearing), ay1 - (tex.Height / 2 + tex.YBearing));
            c.ShowText(line);
        }
        else{
            c.SetSourceColor(pink);
            // displaying score
            c.Rectangle(x: 10, y: 50, width: 100, height: 60);
            c.Rectangle(x: 120, y: 50, width: 100, height: 60);
            c.Rectangle(x: 230, y: 50, width: 100, height: 60);
            c.Rectangle(x: 340, y: 50, width: 100, height: 60);
            c.Rectangle(x: 0, y: 150, width: 450, height: 450);
            c.Fill();
            for(int i=0;i<n;i++){
                for(int j=0;j<n;j++){
                    c.SetSourceColor(colors[gm2048.getTile(i,j)]);
                    c.Rectangle(x: 10+j*sizes[n].l, y: 160+i*sizes[n].l, width: sizes[n].d, height: sizes[n].d);
                    c.Fill();
                    if(gm2048.getTile(i,j)>0){
                        c.SetSourceColor(white);
                        (int ax, int ay) = (10+sizes[n].d/2+sizes[n].l*j, 160+sizes[n].d/2+sizes[n].l*i);
                        string s1 = gm2048.getTile(i,j).ToString();
                        c.SetFontSize(sizes[n].font);
                        TextExtents tex = c.TextExtents(s1);
                        c.MoveTo(ax - (tex.Width / 2 + tex.XBearing), ay - (tex.Height / 2 + tex.YBearing));
                        c.ShowText(s1);
                    }
                    
                }
            }
            // Displaying the Score
            c.SetSourceColor(grey);
            (int ax1, int ay1) = (390,60);
            string s2 = "Score";
            c.SetFontSize(15);
            TextExtents tex1 = c.TextExtents(s2);
            c.MoveTo(ax1 - (tex1.Width / 2 + tex1.XBearing), ay1 - (tex1.Height / 2 + tex1.YBearing));
            c.ShowText(s2);
            (ax1, ay1) = (390,85);
            s2 = gm2048.getScore().ToString();
            c.SetFontSize(20);
            tex1 = c.TextExtents(s2);
            c.MoveTo(ax1 - (tex1.Width / 2 + tex1.XBearing), ay1 - (tex1.Height / 2 + tex1.YBearing));
            c.ShowText(s2);
            // Displaying Repeat button
            c.SetSourceColor(light_green);
            (ax1, ay1) = (280,75);
            s2 = "REPEAT";
            c.SetFontSize(15);
            tex1 = c.TextExtents(s2);
            c.MoveTo(ax1 - (tex1.Width / 2 + tex1.XBearing), ay1 - (tex1.Height / 2 + tex1.YBearing));
            c.ShowText(s2);
            // Displaying Restart button
            c.SetSourceColor(light_green);
            (ax1, ay1) = (170,75);
            s2 = "RESTART";
            c.SetFontSize(15);
            tex1 = c.TextExtents(s2);
            c.MoveTo(ax1 - (tex1.Width / 2 + tex1.XBearing), ay1 - (tex1.Height / 2 + tex1.YBearing));
            c.ShowText(s2);
            // Displaying Restart button
            c.SetSourceColor(blue);
            (ax1, ay1) = (60,75);
            s2 = "MENU";
            c.SetFontSize(15);
            tex1 = c.TextExtents(s2);
            c.MoveTo(ax1 - (tex1.Width / 2 + tex1.XBearing), ay1 - (tex1.Height / 2 + tex1.YBearing));
            c.ShowText(s2);
            // Displaying the game over screen
            if(gm2048.getGameState()){
                c.SetSourceColor(black);
                (int ax, int ay) = (500, 225);
                string s1 = "Game Over!";
                c.SetFontSize(30);
                TextExtents tex = c.TextExtents(s1);
                c.MoveTo(ax - (tex.Width / 2 + tex.XBearing), ay - (tex.Height / 2 + tex.YBearing));
                c.ShowText(s1);
            }
        }
        return true;
    }
    protected override bool OnButtonPressEvent (EventButton e) {
        (double x,double y)= (e.X, e.Y);
        if(menu_){
            if(x<=320 && x>=130 && y<=240 && y>=180){
                gm2048=new MovingBoard(3);
                n=3;
                menu_=!menu_;
            }
            if(x<=320 && x>=130 && y<=330 && y>=270){
                gm2048=new MovingBoard(4);
                n=4;
                menu_=!menu_;
            }
            if(x<=320 && x>=130 && y<=420 && y>=360){
                gm2048=new MovingBoard(5);
                n=5;
                menu_=!menu_;
            }
        }
        else{
            if(x<=330 && x>=230 && y<=110 && y>=50){
                gm2048.repeat();
            }
            if(x<=220 && x>=120 && y<=110 && y>=50){
                gm2048.restart();
            }
            if(x<=110 && x>=10 && y<=110 && y>=50){
                menu_=true;
                gm2048.restart();
            }
        }
        QueueDraw();
        return true;
    }
    public void move(Direction dir){
        gm2048.play(dir);
    }
}
class MyWindow : Gtk.Window {
    View view=new View();
    public MyWindow() : base("2048") {
        Resize(450, 600);
        Add(view);  // add an Area to the window
    }
    protected override bool OnKeyPressEvent(EventKey e) {
        if (e.Key == Key.Left)
            view.move(Left);
        else if (e.Key == Key.Right)
            view.move(Right);
        else if (e.Key == Key.Up)
            view.move(Up);
        else if (e.Key == Key.Down)
            view.move(Down);
        QueueDraw();
        return true;
    }
    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}
class Hello {
    static void Main() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();

    }
}
