import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Button } from 'reactstrap';
import { Link } from 'react-router-dom';
import { actionCreators } from '../store/WikiWalks';
import Head from './Helmet';

class Top extends Component {

    constructor(props) {
        super(props);

        this.state = {
            categories: [],
        }
    }

    componentDidMount() {
        const getData = async () => {
            const url = `api/WikiWalks/getAllCategories`;
            const response = await fetch(url);
            const categories = await response.json();
            this.setState({ categories: categories.slice(0, 100) });
            setTimeout(() => this.setState({ categories }), 1000);
        }
        getData();
    }

    render() {
        const { categories } = this.state;
        const desctiprtion = "このウェブサイトでは、特定のキーワードに関連するWikipedia記事をご紹介させて頂きます。";
        return (
            <div>
                <Head
                    title={"うぃき忍者"}
                    desc={desctiprtion}
                />
                <h1><span style={{ fontWeight: "bold", display: "inline-block" }}>うぃき忍者</span> へ <span style={{ display: "inline-block" }}>ようこそ</span></h1>
                <br />
                <p>
                    調べ物をするなら、世界最高のオンライン辞書「Wikipedia」を使わない手はありません。<br />
                    {desctiprtion}
                </p>
                <br />
                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>カテゴリー</th>
                            <th><span style={{
                                display: "inline-block",
                                minWidth: 100,
                            }}>キーワード</span></th>
                        </tr>
                    </thead>
                    <tbody>
                        {categories.length > 0 ? categories.map(category =>
                            <tr key={category.category}>
                                <td>
                                    {<Link to={"/category/" + encodeURIComponent(category.category.split(" ").join("_"))}>{category.category}</Link>}
                                </td>
                                <td>
                                    <span style={{ display: "inline-block" }}>{category.cnt} keywords</span>
                                </td>
                            </tr>
                        )
                            :
                            <tr><td>Loading...</td><td></td></tr>}
                    </tbody>
                </table>
                <hr />
                <Link to="/all">
                    <center>
                        <Button><b>{"全てのキーワードを確認する"}</b></Button>
                    </center>
                </Link>
                <br />
            </div>
        );
    }
}

export default connect(
    state => state.wikiWalks,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Top);
