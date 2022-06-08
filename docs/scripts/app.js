document.addEventListener("DOMContentLoaded", function()
{
    var checkboxes = document.querySelectorAll('.side-bar input[type=checkbox]');
    for(let chk of checkboxes){
        checkToggle(chk);
    }
    prerpareMain();
});

function checkToggle(chk)
{
    let id = chk.id;
    if(localStorage.getItem('collapse_' + id) == true){
        chk.checked = true;
        console.log('toggle3:', id);
        console.log('collapse3: ', localStorage.getItem('collapse_' + id));

        let parent = chk.parentNode.closest('input[type=checkbox]');
        if(parent)
            checkToggle(parent);
    }
}

function toggleCollapse(event){
    let id = event.target.id;
    let chk = event.target;
    console.log('chk.id: ', chk.id);
    console.log('chk.checked: ', chk.checked);
    localStorage.setItem('collapse_' + id, chk.checked ? 1 : 0);

    // close any below this one if checked
    if(event.target.checked == false){
        for(let sub of chk.querySelectorAll('input[type=checkbox]')){
            if(sub.checked)
                toggleCollapse(sub);
        }
    }
}

function addCopyCodeButton(){
    var cb = document.querySelectorAll('.highlighter-rouge');
    for(let item of cb)
    {
        let ele = document.createElement('div');
        ele.className = 'copy-code';
        item.appendChild(ele);
        ele.addEventListener('click', function() {

            let pre = item.querySelector('pre').innerText;
            navigator.clipboard.writeText(pre);
            ele.className = 'copy-code copied';
            setTimeout(() => {
                ele.className = 'copy-code';
            }, 2000);
        });
    }
}

async function navigateTo(url){
    //try
    {
        const resp = await fetch(url);
        const html = await resp.text();
        console.log('html', html);
        let title = /<h1>(.*?)<\/h1>/.exec(html)[1];
        console.log('title', title);
        html = /<!-- content start -->(.*?)<!-- content end -->/.exec(html)[1];
        console.log('html2', html);
        document.getElementById('main').innerHTML = html;
        window.history.pushState(null, title, url);
    }
    // catch(err) 
    // {
    //     window.location = url;
    // }
}

function captureLinks() {
    var links = document.querySelectorAll('a');
    for(let a of links) {
        
        if(!a.href || (a.href.startsWith('http') && a.href.indexOf('wiki.fileflows.com') < 0))
            continue;
        a.addEventListener('click', function(event) {
            event.stopPropagation();
            event.preventDefault();
            navigateTo(a.href);
        });
    }
}

function prerpareMain(){
    addCopyCodeButton();    
    captureLinks();
}