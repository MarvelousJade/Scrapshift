const fs=require('fs'),vm=require('vm'),assert=require('assert');
const select={value:'well_rounded'},search={value:''},counter={textContent:''};
const tbody={rows:[],replaceChildren(){this.rows=[]},append(row){this.rows.push(row)}};
const doc={querySelector(s){return s==='select'?select:s==='input'?search:s==='tbody'?tbody:counter},createElement(){return {children:[],append(...x){this.children.push(...x)}}}};
vm.runInNewContext(fs.readFileSync('artifacts/appearance-parity/review-script.js','utf8'),{document:doc});
const matrix=JSON.parse(fs.readFileSync('artifacts/appearance-parity/character-item-matrix.json','utf8'));
for(const c of matrix){select.value=c.id;select.onchange();assert.equal(tbody.rows.length,209);}
search.value='Piercing Stabilizer';select.value='well_rounded';search.oninput();assert.equal(tbody.rows.length,1);assert.equal(tbody.rows[0].children[1].textContent,'Visible change');
select.value='knight';select.onchange();assert.equal(tbody.rows[0].children[1].textContent,'Blocked by character priority');
const result={status:'Passed',characters:50,itemRowsPerCharacter:209,search:'Passed',priorityExample:'Piercing Stabilizer changes Rookie; Knight blocks it'};
fs.writeFileSync('artifacts/appearance-parity/lookup-check.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
