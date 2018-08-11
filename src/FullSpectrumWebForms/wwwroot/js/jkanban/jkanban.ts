
declare interface jKanbanOptions {
    element: string | JQuery,                                           // selector of the kanban container
    gutter?: string,                                       // gutter of the board
    widthBoard?: string,                                      // width of the board
    boards: jKanbanBoard[];                                           // json of boards
    dragBoards?: boolean,                                         // the boards are draggable, if false only item can be dragged
    addItemButton?: boolean,                                        // add a button to board for easy item creation
    buttonContent?: string,                                          // text or html content of the board button
    click?: (el: HTMLElement) => void;                             // callback when any board's item are clicked
    dragEl?: (el: HTMLElement, source: any) => void;                     // callback when any board's item are dragged
    dragendEl?: (el: HTMLElement) => void;                             // callback when any board's item stop drag
    dropEl?: (el: HTMLElement, target: any, source: any, sibling: any) => void;    // callback when any board's item drop in a board
    dragBoard?: (el: HTMLElement, source: any) => void,                     // callback when any board stop drag
    dragendBoard?: (el: HTMLElement) => void,                             // callback when any board stop drag
    buttonClick?: (el: HTMLElement, boardId: string) => void                      // callback when the board's button is clicked
}
declare interface jKanbanItem {
    id: string;
    title: string;
}
declare interface jKanbanBoard {
    id: string;
    title: string;
    class: string;
    dragTo?: string[];
    item: jKanbanItem[];
}
declare class jKanban {

    constructor(options: jKanbanOptions);

    addElement(boardId: string, element: jKanbanItem); //Add element in the board with ID boardID, element is the standard format
    addBoards(boards: jKanbanBoard[]);

    findElement(id: string): jKanbanItem;
    findBoard(id: string): jKanbanBoard; // Find board by id
    getBoardElements(): jKanbanBoard[]; // 	Get all item of a board
    removeElement(id: string); // Remove a board's element by id
    removeBoard(id: string); // Remove a board by id
}